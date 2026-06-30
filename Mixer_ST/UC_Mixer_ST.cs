using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mixer_ST
{
  public partial class UC_Mixer_ST : UserControl
  {
    public UC_Mixer_ST()
    {
      InitializeComponent();
    }

    private class BatchItem
    {
      public string PoCode { get; set; } = "";
      public short BatchCode { get; set; }
      public double BatchQuantity { get; set; }
    }

    private readonly List<BatchItem> _buffer = new List<BatchItem>();

    private int _maxBuffer = 7;

    private float _Counter = 0;

    private string _CurrentPoCode = "";
    private short _CurrentBatchCode = 0;
    private float _CurrentBatchQuantity = 0;

    private string _2nd_PoCode = "";
    private short _2nd_BatchCode = 0;
    private float _2nd_BatchQuantity = 0;

    private string _3rd_PoCode = "";
    private short _3rd_BatchCode = 0;
    private float _3rd_BatchQuantity = 0;

    private double _batchStartActualCounter = 0;
    private double _tempBatchCounter = 0;
    private double _lastValidCounter = 0;
    private bool _hasRunningPo = false;

    public void AddBuffer(string poCode, short batchCode, double batchQuantity)
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new Action(() =>
        {
          AddBuffer(poCode, batchCode, batchQuantity);
        }));
        return;
      }

      //if (string.IsNullOrWhiteSpace(poCode))
      //  return;

      //if (batchQuantity <= 0)
      //  batchQuantity = 1;

      //UpdateActualCounterWithRemainder(_Counter);

      //if (IsBatchAlreadyInBuffer(poCode, batchCode, batchQuantity))
      //  return;

      //if (_buffer.Count >= _maxBuffer)
      //  return;
      if (string.IsNullOrWhiteSpace(poCode))
        return;

      if (batchQuantity <= 0)
        batchQuantity = 1;

      batchQuantity = Math.Ceiling(batchQuantity);

      UpdateActualCounterWithRemainder(_Counter);

      if (IsBatchAlreadyInBuffer(poCode, batchCode, batchQuantity))
        return;

      if (_buffer.Count >= _maxBuffer)
        return;

      if (_buffer.Count == 0)
      {
        SetBatchStartActualCounter(_Counter);
        _hasRunningPo = true;
      }

      _buffer.Add(new BatchItem
      {
        PoCode = poCode,
        BatchCode = batchCode,
        BatchQuantity = batchQuantity
      });

      UpdateCurrentRunningBatch();
    }

    private bool IsBatchAlreadyInBuffer(string poCode, short batchCode, double batchQuantity)
    {
      foreach (BatchItem item in _buffer)
      {
        bool samePoCode = item.PoCode == poCode;
        bool sameBatchCode = item.BatchCode == batchCode;
        bool sameQuantity = Math.Abs(item.BatchQuantity - batchQuantity) < 0.0001;

        if (samePoCode && sameBatchCode && sameQuantity)
          return true;
      }

      return false;
    }

    private double GetCurrentBatchQuantity()
    {
      if (_buffer.Count == 0)
        return 0;

      return _buffer[0].BatchQuantity;
    }

    private void MoveNextBatch()
    {
      if (_buffer.Count > 0)
      {
        _buffer.RemoveAt(0);
      }

      UpdateCurrentRunningBatch();
    }

    public void BypassCurrentBatch(double actualCounter)
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new Action(() =>
        {
          BypassCurrentBatch(actualCounter);
        }));
        return;
      }

      if (_buffer.Count == 0)
        return;

      MoveNextBatch();

      if (GetCurrentBatchQuantity() > 0)
      {
        SetBatchStartActualCounter(actualCounter);
        _hasRunningPo = true;
      }
      else
      {
        _batchStartActualCounter = 0;
        _tempBatchCounter = 0;
        _lastValidCounter = actualCounter;
        _hasRunningPo = false;
      }

      UpdateCurrentRunningBatch();
    }

    public void SetBatchStartActualCounter(double actualCounter)
    {
      _batchStartActualCounter = actualCounter;
      _lastValidCounter = actualCounter;
      _tempBatchCounter = 0;
    }

    public void UpdateActualCounterWithRemainder(double actualCounter)
    {
      UpdateCurrentRunningBatch();

      double currentBatchQuantity = GetCurrentBatchQuantity();

      if (currentBatchQuantity <= 0)
      {
        _hasRunningPo = false;
        _tempBatchCounter = 0;
        UpdateCurrentRunningBatch();
        return;
      }

      if (!_hasRunningPo)
      {
        SetBatchStartActualCounter(actualCounter);
        _hasRunningPo = true;
      }

      if (actualCounter < _lastValidCounter)
      {
        return;
      }

      _lastValidCounter = actualCounter;

      while (true)
      {
        currentBatchQuantity = GetCurrentBatchQuantity();

        if (currentBatchQuantity <= 0)
        {
          _hasRunningPo = false;
          _tempBatchCounter = 0;
          UpdateCurrentRunningBatch();
          return;
        }

        _tempBatchCounter = actualCounter - _batchStartActualCounter;

        if (_tempBatchCounter < currentBatchQuantity)
        {
          UpdateCurrentRunningBatch();
          return;
        }

        MoveNextBatch();

        _batchStartActualCounter += currentBatchQuantity;

        _tempBatchCounter = actualCounter - _batchStartActualCounter;

        UpdateCurrentRunningBatch();
      }
    }


    private void UpdateCurrentRunningBatch()
    {
      _CurrentPoCode = "";
      _CurrentBatchCode = 0;
      _CurrentBatchQuantity = 0;

      _2nd_PoCode = "";
      _2nd_BatchCode = 0;
      _2nd_BatchQuantity = 0;

      _3rd_PoCode = "";
      _3rd_BatchCode = 0;
      _3rd_BatchQuantity = 0;

      if (_buffer.Count == 0)
      {
        _CurrentPoCode = "";
        _CurrentBatchCode = 0;
        _CurrentBatchQuantity = 0;
        return;
      }

      //Test


      //Viet them

      BatchItem current = _buffer[0];

      _CurrentPoCode = current.PoCode;
      _CurrentBatchCode = current.BatchCode;
      _CurrentBatchQuantity = (float)current.BatchQuantity;

      if (_buffer.Count >= 2)
      {
        BatchItem second = _buffer[1];

        _2nd_PoCode = second.PoCode;
        _2nd_BatchCode = second.BatchCode;
        _2nd_BatchQuantity = (float)second.BatchQuantity;
      }

      if (_buffer.Count >= 3)
      {
        BatchItem third = _buffer[2];

        _3rd_PoCode = third.PoCode;
        _3rd_BatchCode = third.BatchCode;
        _3rd_BatchQuantity = (float)third.BatchQuantity;
      }
    }

    public void ClearBuffer()
    {
      if (this.InvokeRequired)
      {
        this.Invoke(new Action(() =>
        {
          ClearBuffer();
        }));
        return;
      }

      _buffer.Clear();

      _batchStartActualCounter = 0;
      _tempBatchCounter = 0;
      _lastValidCounter = 0;
      _hasRunningPo = false;


      _CurrentPoCode = "";
      _CurrentBatchCode = 0;
      _CurrentBatchQuantity = 0;

      _2nd_PoCode = "";
      _2nd_BatchCode = 0;
      _2nd_BatchQuantity = 0;

      _3rd_PoCode = "";
      _3rd_BatchCode = 0;
      _3rd_BatchQuantity = 0;


      UpdateCurrentRunningBatch();

    }

    public double GetTempBatchCounter()
    {
      return _tempBatchCounter;
    }

    public double GetBatchStartActualCounter()
    {
      return _batchStartActualCounter;
    }

    public double GetCurrentBatchProgressPercent()
    {
      double currentBatchQuantity = GetCurrentBatchQuantity();

      if (currentBatchQuantity <= 0)
        return 0;

      return (_tempBatchCounter / currentBatchQuantity) * 100.0;
    }

    [Browsable(true)]
    [Category("Custom")]
    public float Counter
    {
      get => _Counter;
      set
      {
        if (_Counter == value)
          return;

        _Counter = value;

        UpdateActualCounterWithRemainder(_Counter);
      }
    }

    [Browsable(true)]
    [Category("Custom")]
    public string CurrentPoCode
    {
      get => _CurrentPoCode;
    }

    [Browsable(true)]
    [Category("Custom")]
    public short CurrentBatchCode
    {
      get => _CurrentBatchCode;
    }

    [Browsable(true)]
    [Category("Custom")]
    public float CurrentBatchQuantity
    {
      get => _CurrentBatchQuantity;
    }

    [Browsable(true)]
    [Category("Custom")]
    public string PoCode_2nd
    {
      get => _2nd_PoCode;
    }

    [Browsable(true)]
    [Category("Custom")]
    public short BatchCode_2nd
    {
      get => _2nd_BatchCode;
    }

    [Browsable(true)]
    [Category("Custom")]
    public float BatchQuantity_2nd
    {
      get => _2nd_BatchQuantity;
    }

    [Browsable(true)]
    [Category("Custom")]
    public string PoCode_3rd
    {
      get => _3rd_PoCode;
    }

    [Browsable(true)]
    [Category("Custom")]
    public short BatchCode_3rd
    {
      get => _3rd_BatchCode;
    }

    [Browsable(true)]
    [Category("Custom")]
    public float BatchQuantity_3rd
    {
      get => _3rd_BatchQuantity;
    }

    [Browsable(true)]
    [Category("Custom")]
    public int BufferCount
    {
      get => _buffer.Count;
    }

    [Browsable(true)]
    [Category("Custom")]
    public double CurrentBatchCounter
    {
      get => _tempBatchCounter;
    }
  }
}
