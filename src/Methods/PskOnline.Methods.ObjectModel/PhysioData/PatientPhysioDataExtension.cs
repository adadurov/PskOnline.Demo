using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PskOnline.Methods.ObjectModel.PhysioData
{
  public static class PatientPhysioDataExtension
  {
    private const double OneM = 1000000.0;

    /// <summary>
    /// Возвращает true если физиологические данные
    /// содержат маркер с указанным идентификатором.
    /// </summary>
    public static bool ContainsMarker(this PatientPhysioData _this, long markerId)
    {
      return _this.Markers.ContainsKey(markerId);
    }

    /// <summary>
    /// Возвращает время от начала измерения
    /// до создания данного маркера.
    /// </summary>
    /// <param name="markerId"></param>
    /// <returns></returns>
    public static TimeSpan GetAbsoluteTimeForMarker(this PatientPhysioData _this, long markerId)
    {
      Marker marker = _this.GetMarkerByIdOrDefault(markerId);
      return new TimeSpan(marker.TimestampUsec * 10);
    }

    public static ChannelMark GetChannelMarkByChannelName(
      ChannelMark[] channelMarks, string channelName)
    {
      return channelMarks.FirstOrDefault(cm => cm.ChannelId == channelName);
    }

    /// <summary>
    /// Возвращает время от получения первого отсчета в данном канале
    /// до создания данного маркера
    /// </summary>
    /// <param name="markerId"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    public static TimeSpan GetChannelTimeForMarker(
      this PatientPhysioData _this, long markerId, ChannelData channel)
    {
      Marker marker = _this.GetMarkerByIdOrDefault(markerId);
      System.Diagnostics.Debug.Assert(marker != null);
      ChannelMark cm = GetChannelMarkByChannelName(marker.ChannelMarks, channel.ChannelId);

      // Длина канала в микросекундах
      double channelDataMicroseconds = ((double)channel.Data.Length) / channel.BitsPerSample * OneM;
      // Положение метки относительно начала данных канала в микросекундах
      double channelMarkerMicroseconds = ((double)cm.Count) / channel.BitsPerSample * OneM + (double)cm.Offset;
      if (channelMarkerMicroseconds < 0)
      {
        System.Diagnostics.Debug.Assert(false);
        // такая ситуация может возникнуть в двух случаях
        // 1. когда метка поставлена в начале отсчета временной шкалы измерения,
        // а за время выполнения цикла создания меток каналов в одном из каналов
        // появились новые данные, при этом смещение метки канала будет отрицательным,
        // а абсолютное значение смещения может быть больше чем (cm.Count / channel.BitsPerSample * 1000000.0)
        // 2. когда метка ставится на канал, в котором еще нет данных

        channelMarkerMicroseconds = 0;
      }

      // исправить
      //return new System.TimeSpan( (marker.TimestampUsec - cm.Offset) * 10 );
      // на 
      return new TimeSpan((long)Math.Floor(Math.Min(channelMarkerMicroseconds, channelDataMicroseconds)) * 10);
    }

    /// <summary>
    /// Создает метку при пост-обработке данных
    /// Метка будет соответствовать количеству данных в каждом из каналов
    /// на момент создания данной метки.
    /// Метка будет добавлена в массив меток (соответственно,
    /// массив меток будет заменен на новый экземпляр).
    /// </summary>
    /// <returns></returns>
    public static Marker CreateMarkerOffline(this PatientPhysioData _this)
    {
      var channels = _this.Channels;
      var channelMarks = new ChannelMark[channels.Count];
      long timestampUsec = 0;
      int i = 0;
      foreach (ChannelData channel in channels.Values)
      {
        ChannelMark cm = new ChannelMark
        {
          ChannelId = channel.ChannelId,
          // Время, прошедшее с момента получения последней метки времени.
          Offset = 0,
          // Кол-во данных в буфере канала, соответствующее последней метке времени.
          Count = channel.Data.Length
        };

        channelMarks[i++] = cm;

        // Кол-во микросекунд, прошедших с начала записи данных
        // (результат фиксируется по последнему каналу).
        timestampUsec = (long)(OneM * channel.Data.Length / channel.SamplingRate);
      }

      // Увеличиваем номер маркера.
      ++_this.MaxMarkerId;

      Marker marker = new Marker
      {
        Id = _this.MaxMarkerId,
        ChannelMarks = channelMarks,
        TimestampUsec = timestampUsec
      };

      _this.Markers[marker.Id] = marker;

      return marker;
    }

    /// <summary>
    /// Смещает метку на заданное кол-во микросекунд.
    /// Максимальное положение метки на оси времени не контролируется.
    /// </summary>
    /// <param name="markerId">Идентификатор метки</param>
    /// <param name="markerOffset">Смещение метки в микросекундах</param>
    public static void MoveMarker(
      this PatientPhysioData _this, long markerId, long markerOffset)
    {
      if (!_this.ContainsMarker(markerId))
      {
        return;
      }

      var marker = _this.GetMarkerByIdOrDefault(markerId);

      // нет смысла двигать маркер, если он установлен на "минус бесконечность"
      if (long.MinValue == marker.TimestampUsec)
      {
        return;
      }

      // метка не может быть установлена раньше начала записи данных
      if ((marker.TimestampUsec + markerOffset) < 0)
      {
        markerOffset = 0 - marker.TimestampUsec;
      }

      // модифицировать канальные метки, соответствующие данному маркеру
      // максимальное положение метки на оси времени не контролируется
      foreach (ChannelMark channelMark in marker.ChannelMarks)
      {
        var channelData = _this.Channels[channelMark.ChannelId];
        System.Diagnostics.Debug.Assert(channelData != null);

        double channelMarkerOffset = _this.MoveChannelMarker(channelMark, markerOffset, channelData);

        // разница между требуемым смещением метки канала и реально полученным
        // может быть меньше нуля только тогда, когда требуемое новое положение метки
        // лежит на временной оси раньше начала данных канала.
        double markerOffsetDelta = ((double)markerOffset) - channelMarkerOffset;
        // дополняем смещение метки канала, невыходящее за границы временного интервала данных,
        // смещением до заданного нового положения метки синхронизации.
        channelMark.Offset += (long)Math.Floor(markerOffsetDelta);
      }

      // новое положение метки синхронизации
      marker.TimestampUsec += markerOffset;
    }

    /// <summary>
    /// </summary>
    /// <param name="markerId"></param>
    /// <returns></returns>
    public static Marker GetMarkerByIdOrDefault(this PatientPhysioData _this, long markerId)
    {
      return _this.Markers.ContainsKey(markerId) ? _this.Markers[markerId] : null;
    }

    /// <summary>
    /// Смещает метку канала на заданное кол-во микросекунд.
    /// Возвращает смещение в микросекундах,
    /// на которое реально была перемещена метка канала.
    /// </summary>
    /// <param name="markerId">Идентификатор метки</param>
    /// <param name="markerOffset">Смещение метки в микросекундах</param>
    /// <param name="channelName">Имя канала данных</param>
    public static double MoveChannelMarker(
      this PatientPhysioData _this,
      long markerId,
      long markerOffset,
      string channelName)
    {
      if (!_this.ContainsMarker(markerId))
      {
        return 0;
      }

      var marker = _this.GetMarkerByIdOrDefault(markerId);

      var channelMarker = GetChannelMarkByChannelName(marker.ChannelMarks, channelName);
      if (null == channelMarker)
      {
        return 0;
      }

      var channel = _this.GetChannelData(channelName);
      if (null == channel)
      {
        return 0;
      }

      return _this.MoveChannelMarker(channelMarker, markerOffset, channel);
    }

    /// <summary>
    /// Смещает метку канала на заданное кол-во микросекунд.
    /// Возвращает смещение в микросекундах,
    /// на которое реально была перемещена метка канала.
    /// </summary>
    /// <param name="markerId">Идентификатор метки</param>
    /// <param name="markerOffset">Смещение метки в микросекундах</param>
    /// <param name="channel">Канал данных</param>
    public static double MoveChannelMarker(this PatientPhysioData _this, long markerId, long markerOffset, ChannelData channel)
    {
      if (!_this.ContainsMarker(markerId))
      {
        return 0;
      }

      var marker = _this.GetMarkerByIdOrDefault(markerId);
      if (null == marker)
      {
        return 0;
      }

      var channelMarker = GetChannelMarkByChannelName(marker.ChannelMarks, channel.ChannelId);
      if (null == channelMarker)
      {
        return 0;
      }

      return _this.MoveChannelMarker(channelMarker, markerOffset, channel);
    }

    /// <summary>
    /// Смещает метку канала на заданное кол-во микросекунд.
    /// Возвращает смещение в микросекундах,
    /// на которое реально была перемещена метка канала.
    /// </summary>
    /// <param name="marker">Метка канала</param>
    /// <param name="offset">Смещение метки в микросекундах</param>
    /// <param name="channel">Канал данных</param>
    public static double MoveChannelMarker(
      this PatientPhysioData _this,
      ChannelMark marker,
      long offset,
      ChannelData channel)
    {
      // Частота оцифровки канала
      double samplingRate = channel.SamplingRate;
      // Кол-во данных канала
      double dataCount = channel.Data.Length;
      // Длина канала в микросекундах
      double dataMicroseconds = dataCount / samplingRate * OneM;


      // Положение метки во времени относительно начала данных канала в микросекундах
      double markerPosition = ((double)marker.Count) / samplingRate * OneM + (double)marker.Offset;

      // Новое положение метки на канале в микросекундах
      double newMarkerPosition = markerPosition + (double)offset;

      if (newMarkerPosition < 0)
      {
        newMarkerPosition = 0;
      }
      else if (newMarkerPosition > dataMicroseconds)
      {
        newMarkerPosition = dataMicroseconds;
      }

      // Новое кол-во отсчетов метки
      marker.Count = (long)Math.Floor(newMarkerPosition * samplingRate / OneM);
      // Новое смещение метки	в микросекундах
      marker.Offset = (long)Math.Floor(newMarkerPosition - ((double)marker.Count) / samplingRate * OneM);


      // Новое положение метки на канале с учетом Math.Floor
      newMarkerPosition = ((double)marker.Count) / samplingRate * OneM + (double)marker.Offset;

      // Смещение в микросекундах, на которое реально была перемещена метка канала
      return newMarkerPosition - markerPosition;
    }

    /// <summary>
    /// Возвращает буфер для данных канала.
    /// </summary>
    /// <param name="channelId">
    /// Полное (включая пространство имен) имя канала
    /// </param>
    /// <returns></returns>
    public static ChannelData GetChannelData(this PatientPhysioData _this, string channelId)
    {
      return _this.Channels.ContainsKey(channelId) ? _this.Channels[channelId] : null;
    }

    public static long GetChannelDataCountForMarker(
      this PatientPhysioData _this, ChannelData data, long markerId)
    {
      Marker marker = _this.GetMarkerByIdOrDefault(markerId);
      if (null != marker)
      {
        ChannelMark m = GetChannelMarkByChannelName(marker.ChannelMarks, data.ChannelId);
        if (null != m)
        {
          return m.Count + ((long)(((double)m.Offset) * data.SamplingRate / OneM));
        }
      }
      return -1;
    }

    public static ChannelData GetChannelDataFromLeftToRight(
      this PatientPhysioData _this,
      string channelName,
      long leftMarkerId,
      long rightMarkerId)
    {
      var channelData = _this.GetChannelData(channelName);
      return _this.GetChannelDataFromLeftToRight(channelData, leftMarkerId, 0, rightMarkerId, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="leftMarkerId"></param>
    /// <param name="leftOffset">смещение относительно левого маркера (начала)</param>
    /// <param name="rightMarkerId"></param>
    /// <param name="rightOffset">смещение относительно правого маркера (конца)</param>
    /// <returns></returns>
    public static ChannelData GetChannelDataFromLeftToRight(
      this PatientPhysioData _this,
      string channelName,
      long leftMarkerId,
      long leftOffset,
      long rightMarkerId,
      long rightOffset)
    {
      var channelData = _this.GetChannelData(channelName);
      return _this.GetChannelDataFromLeftToRight(channelData, leftMarkerId, leftOffset, rightMarkerId, rightOffset);
    }

    public static ChannelData GetChannelDataFromLeftToRight(
      this PatientPhysioData _this,
      ChannelData channelData,
      long leftMarkerId,
      long rightMarkerId)
    {
      return _this.GetChannelDataFromLeftToRight(channelData, leftMarkerId, 0, rightMarkerId, 0);
    }

    public static ChannelData GetChannelDataFromLeftToRight(
      this PatientPhysioData _this,
      ChannelData channelData,
      long leftMarkerId,
      long leftMarkerOffset,
      long rightMarkerId,
      long rightMarkerOffset)
    {
      if (null == channelData)
      {
        return null;
      }

      Marker leftMarker = _this.GetMarkerByIdOrDefault(leftMarkerId);
      Marker rightMarker = _this.GetMarkerByIdOrDefault(rightMarkerId);

      // Вырезать данные и вернуть их.
      return _this.CutChannelDataFromLeftToRight(channelData, leftMarker, rightMarker);
    }


    public static PatientPhysioData GetFromLeft(this PatientPhysioData _this, long leftMarkerId)
    {
      return _this.GetFromLeftToRight(leftMarkerId, 0);
    }

    public static PatientPhysioData GetToRight(this PatientPhysioData _this, long rightMarkerId)
    {
      return _this.GetFromLeftToRight(0, rightMarkerId);
    }

    public static PatientPhysioData GetFromLeftToRight(this PatientPhysioData _this, long leftMarkerId, long rightMarkerId)
    {
      Marker leftMarker = _this.GetMarkerByIdOrDefault(leftMarkerId);
      Marker rightMarker = _this.GetMarkerByIdOrDefault(rightMarkerId);

      // Вырезать данные и вернуть их.
      return _this.CutPhysioDataFromLeftToRight(leftMarker, rightMarker);
    }

    public static ChannelData GetFirstChannelDataByPhysioSignalTypeName(
      this PatientPhysioData _this, SignalType physioSignalType)
    {
      return _this.GetChannelDataBySignalType(physioSignalType).FirstOrDefault();
    }


    /// <summary>
    /// защита от неправильного использования отсутствует,
    /// поэтому нельзя делать эту функцию public
    /// </summary>
    public static ChannelData CutChannelDataFromLeftToRight(
      this PatientPhysioData _this, ChannelData channel_data, Marker left_marker, Marker right_marker)
    {
      System.Diagnostics.Debug.Assert(channel_data != null);

      lock (channel_data)
      {
        int left_index = 0;
        int right_index = channel_data.Data.Length - 1;

        if (left_marker != null)
        {
          ChannelMark lcm = GetChannelMarkByChannelName(left_marker.ChannelMarks, channel_data.ChannelId);
          System.Diagnostics.Debug.Assert(lcm != null);
          double offsetInSeconds = ((double)lcm.Offset) / OneM;
          long offsetInSamples = (long)Math.Floor(offsetInSeconds * channel_data.SamplingRate);

          left_index = (int)Math.Max(lcm.Count + offsetInSamples - 1, left_index);
        }

        if (right_marker != null)
        {
          ChannelMark rcm = GetChannelMarkByChannelName(right_marker.ChannelMarks, channel_data.ChannelId);
          System.Diagnostics.Debug.Assert(rcm != null);

          double offsetInSeconds = ((double)rcm.Offset) / OneM;
          long offsetInSamples = (long)Math.Floor(offsetInSeconds * channel_data.SamplingRate);

          right_index = (int)Math.Min(rcm.Count + offsetInSamples - 1, right_index);
        }

        return new ChannelData(channel_data, left_index, right_index);
      }
    }

    /// <summary>
    /// Возвращает фрагмент сигнала из всех каналов нужного типа от маркера left до маркера right.
    /// </summary>
    /// <param name="physioSignalType">нужный тип физиологического сигнала.</param>
    /// <param name="leftMarkerId"></param>
    /// <param name="rightMarkerId"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static IEnumerable<ChannelData> GetChannelDataBySignalType(
      this PatientPhysioData _this,
      SignalType physioSignalType,
      long leftMarkerId,
      long rightMarkerId)
    {
      // Берем сигналы нужного типа целиком
      var channels = _this.GetChannelDataBySignalType(physioSignalType);

      // Найти объекты-маркеры, соответствующие left_marker и right_marker
      Marker leftMarker = _this.GetMarkerByIdOrDefault(leftMarkerId);
      Marker rightMarker = _this.GetMarkerByIdOrDefault(rightMarkerId);

      var fragments = channels.Select( 
        c => _this.CutChannelDataFromLeftToRight(c, leftMarker, rightMarker) 
        );
      return fragments;
    }

    /// <summary>
    /// Возвращает сигналы по типу физиологических данных.
    /// </summary>
    /// <param name="physioSignalType">Тип требуемого физиологического сигнала.</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static IEnumerable<ChannelData> GetChannelDataBySignalType(this PatientPhysioData _this, SignalType physioSignalType)
    {
      if (physioSignalType == SignalType.Any)
      {
        return _this.Channels.Values;
      }
      return _this.Channels.Values
        .Where(c => c.PhysioSignalType == physioSignalType)
        .Select( c => new ChannelData(c));
    }

    /// <summary>
    /// защита от неправильного использования отсутствует,
    /// поэтому нельзя делать эту функцию public
    /// </summary>
    public static PatientPhysioData CutPhysioDataFromLeftToRight(
      this PatientPhysioData _this, Marker leftMarker, Marker rightMarker)
    {
      System.Diagnostics.Debug.Assert(_this != null);

      var newPhysioData = new PatientPhysioData();

      // Копируем все сигналы из исходных физиологических данных
      foreach (ChannelData channel_data in _this.Channels.Values)
      {
        var channelFragment = _this.CutChannelDataFromLeftToRight(channel_data, leftMarker, rightMarker);
        newPhysioData.Channels[channel_data.ChannelId] = channelFragment;
      }

      // вот на сколько микросекунд все сдвигается влево:
      long microseconds = 0;
      if (leftMarker != null)
      {
        microseconds = leftMarker.TimestampUsec;
      }

      // Модифицируем информацию о метках
      foreach (Marker marker in _this.Markers.Values)
      {
        if (marker.TimestampUsec < microseconds)
        {
          // пропускаем метки, которые созданы раньше чем left_marker
          continue;
        }

        if (rightMarker != null && rightMarker.TimestampUsec < marker.TimestampUsec)
        {
          // пропускаем метки, которые созданы позже чем right_marker
          continue;
        }

        // создаем копию метки
        Marker newMarker = new Marker(marker);
        // добавляем метку
        newPhysioData.Markers[newMarker.Id] = newMarker;
        // .. и смещаем метку на microseconds влево
        newPhysioData.MoveMarker(newMarker.Id, 0 - microseconds);
      }

      return newPhysioData;
    }

  }
}
