namespace PskOnline.Math.Psa
{
  using System;
  using System.Text;

  /// <summary>
  /// Собирает и хранит статистику ряда для
  /// указанного количества последних членов ряда
  /// 
  /// При добавлени первого значения статистика приводится в следующее состояния:
  /// история значения устанавливается в первое значение, поданное на вход
  /// мат. ожидание x
  /// дисперсия 0
  /// стандартное отклонение 0
  /// </summary>
  public class RingBuffer<T>
  {
    public RingBuffer(int _size)
    {
      this.size = _size;
      this.buffer = new T[this.size];
      this.pointer = 0;
    }

    public int GetSize()
    {
      return this.size;
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder(this.GetSize() * 8);
      sb.Append("[ ");
      for( int i = this.GetSize() - 1; i > -1; --i )
      {
        sb.AppendFormat("{0} ", this.GetValue(i).ToString());
      }
      sb.Append("]");
      return sb.ToString();
    }

    /// <summary>
    /// fills the entire buffer with the given value
    /// 
    /// does not check if initialization was already performed before
    /// </summary>
    /// <param name="value"></param>
    public void InitBuffer(T value)
    {
      for (int i = 0; i < this.size; ++i)
      {
        this.buffer[i] = value;
      }
    }

    private void IncrementPointers()
    {
      this.pointer = this.AdvancePointer(this.pointer, 1);
    }

    private int AdvancePointer(int initial_value, int offset)
    {
      System.Diagnostics.Debug.Assert(offset >= 0);

      return (initial_value + offset) % this.size;
    }

    /// <summary>
    /// adds new point and deletes the oldest one
    /// </summary>
    /// <param name="value"></param>
    public virtual void AddValue(T value)
    {
      this.buffer[this.pointer] = value;
      IncrementPointers();
    }

    public T GetValue(int reverse_offset)
    {
      if (!this.IsPointInHistoryBuffer(reverse_offset))
      {
        string msg = string.Format(
          "reverse_offset={0} is out of valid range [{1}, {2}]",
          reverse_offset, 
          0, 
          this.GetSize() - 1
          );
        throw new ArgumentException(msg);
      }
      return this.buffer[(this.pointer - reverse_offset - 1 + this.size) % this.size];
    }

    internal T GetNewestValue()
    {
      return this.GetValue(0);
    }

    public T GetOldestValue()
    {
      return this.GetValue(this.GetSize() - 1);
    }

    public bool IsPointInHistoryBuffer(int reverse_offset)
    {
      return (reverse_offset < this.size) && (reverse_offset > -1);
    }

    T[] buffer;
    int pointer;
    int size;



  }
}
