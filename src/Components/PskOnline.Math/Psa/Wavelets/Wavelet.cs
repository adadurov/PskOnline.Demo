using System;
using System.Collections.Generic;
using System.Text;

namespace PskOnline.Math.Psa.Wavelets
{
  /// <summary>
  /// �����, �������������� �������-�������������� (���������������, ������������ � ������, ����������, undecimated).
  /// �������������� ������� ���� ����� ���� �������� ��� ���������� ������� � ������ ��������� �������.
  /// 
  /// � �������������� ������� ������� ��� �������� ���������: ������� � ���������� �-� ��������������� ��������. 
  /// �-�� ��������������� ���������� �� ���������� ��������� ������, ��������� ������������� �� ��������� ������ �� 
  /// �����-���� �����������. ������� - ������������� ����� ����� - ���������� � ������������ � ����� ���������������
  /// �������. ��� ���� �������, ��� ������� ��������� ����� ������� ����������� ��������������, ����� �����������
  /// ������������� ����� �������������� ���������� �������. ������������ � ���� ������ �������� ����� ������ � 
  /// ������� �������.
  /// 
  /// ��������� ���������� ������������ � ������ FIFO: �� ���� �������� �����, � ������ ���������� �����.
  /// ���������� �������������� ��������, ��������� �� ���������� � �������������, �������� ���������, ������� 
  /// �������������� ������� ������������, �.�. �������� � ������ ����� �� ��������. ���� �� �������� �������� �����,
  /// �� �������� �� ���� ����������. ����� �������� ���������� �������� �������� ���������� ������ ����� ���������� �����, 
  /// ������ ��������.
  /// </summary>
  public class Wavelet
  {
    #region private & protected

    /// <summary>
    /// ������� ������ 1:1
    /// </summary>
		private float[] x;  
    
    /// <summary>
    /// �� ����������
    /// </summary>
    private float[] v;  

    /// <summary>
    /// ����-�� ��� �������������
    /// </summary>
    private float[] iv; 

    #endregion
    
    #region protected 
		
    /// <summary>
    /// �� ����-�� ����������
    /// </summary>
    protected float[] w;

    /// <summary>
    /// �-� ��������
    /// </summary>
    protected float[] h;

    /// <summary>
    /// �-� ���������������
    /// </summary>
    protected float[] g;

    /// <summary>
    /// ������� ��������
    /// </summary>
    protected int level;

    /// <summary>
    /// ������ �-�� ��������������� (����� �� �����)
    /// </summary>
    protected int n;

    /// <summary>
    /// ����� ����������� ��������
    /// </summary>
    protected int length;

    /// <summary>
    /// ������� ���������
    /// </summary>
    protected int position;

    /// <summary>
    /// ���������� ��� ���������� ������� ������.
    /// </summary>
    protected void CleanupMemory()
    {
    }

    #endregion

    #region public

    /// <summary>
    /// ������������� ��������
    /// </summary>
    /// <param name="level">������� ��������</param>
    /// <param name="n">������ �������� (��������� � ������)</param>
    /// <param name="g">������ ����� �-�� ��������������� ������ idx</param>
    public Wavelet(int level, int n, float[] g)
    {
      int i;

      this.level = level;
      this.n = n;

      length = ((1 << (level)) - 1) * (n - 1) + 1;
      position = 0;

      this.x = new float[length];
      this.v = new float[length * level];
      this.w = new float[length * level];
      this.iv = new float[length * level];
      this.g = new float[n];
      this.h = new float[n];

      Reset();

      for (i = n - 1; i >= 0; i--)
      {
        h[i] = (i % 2 == 0 ? 1 : -1) * g[n - 1 - i];
        this.g[i] = g[i];
      }
    }

    /// <summary>
    /// �������� ���������� ������.
    /// </summary>
    public void Reset()
    {
      int i;
      for (i = length - 1; i >= 0; i--)
        x[i] = 0;

      for (i = length * level - 1; i >= 0; i--)
      {
        v[i] = 0;
        w[i] = 0;
        iv[i] = 0;
      }
    }

    /// <summary>
    /// �������� ����� �����.
    /// </summary>
    /// <param name="val">��������</param>
    public virtual void AddPoint(float val)
    {
      float[] da;
      int d;

      float ws, vs;
      int i, j, p1, step, l;

      position++;
      if (position >= length)
      {
        position -= length;
      }

      // �������� ����� �����
      x[position] = val;

      // ��������� ��������� �� ������� ������
      d = 0;
      da = this.x;

      // ����� i ���������� ������� ��� ��� 2^i. ���������� ���������� � ����� �� �����
      // � ����� ��������� � ����������� ����. ��������, � ������������ ��������� �� ������� ������
      for (i = 0; i < level; i++)
      {
        step = 1 << i;
        p1 = position;
        ws = 0;
        vs = 0;
        for (j = 0; j < n; j++)
        {
          ws += h[j] * da[d + p1];
          vs += g[j] * da[d + p1];
          p1 -= step;
          if (p1 < 0)
          {
            p1 += length;
          }
        }
        l = level - i - 1;
        v[l * length + position] = vs;
        w[l * length + position] = ws;
        da = this.v;
        d = l * length;
      }
    }
    
    /// <summary>
    /// �������� �����
    /// </summary>
    /// <returns>��������������� ��������</returns>
    /// <remarks>������������ ������������� � �������� �������.
    /// ����-�� ������� ���������� ������ ����� ���������, ���� ������ ����������� ���-�� ����-��� �����������, � ������ 2^l. ��� � ������ ��������� ��������.
    /// </remarks>
    public virtual float GetPoint()
    {
      int offset, l, step, i, p1, p2;
      float vs = 0;

      float[] da;
      int d;

      da = this.v;
      d = 0;

      offset = 0;

      for (l = 0; l < level; l++)
      {
        step = 1 << (level - l - 1);
        offset += step * (n - 1);
        vs = 0;
        p1 = position - offset;
        if (p1 < 0)
        {
          p1 += length;
        }

        p2 = p1;

        for (i = 0; i < n; i++)
        {
          vs += (w[l * length + p1] * h[i] + da[d+p1] * g[i]) / 2;
          p1 += step;
          if (p1 >= length)
          {
            p1 -= length;
          }
        }
        iv[l * length + p2] = vs;

        da = this.iv;
        d = l * length;
      }

      return vs;
    }

    /// <summary>
    /// ���������� �������� �������
    /// </summary>
    /// <returns></returns>
    public int GetLatency()
    {
      return length - 1;
    }

    #endregion
  }
}
