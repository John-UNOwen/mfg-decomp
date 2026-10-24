using System;
using System.Collections.Generic;
using System.Threading;
using KAMUNITY.AVS;

namespace KAMUNITY.THREADING;

public class AvsThread
{
	private class Parameter
	{
		public ThreadStart start;

		public uint sz_stack;

		public AvsTypes.AVS_THREAD_PRIORITY priority;
	}

	private static object lock_obj = new object();

	private static int counter = 0;

	private static Dictionary<int, AvsThread> invoke_list = new Dictionary<int, AvsThread>();

	private Parameter parameter = new Parameter();

	private int id;

	private int thread_id;

	public ThreadState _ThreadState = ThreadState.Unstarted;

	public bool IsAlive => ThreadState == ThreadState.Running;

	public AvsTypes.AVS_THREAD_PRIORITY priority
	{
		get
		{
			return parameter.priority;
		}
		set
		{
			parameter.priority = value;
			Avs.AvsThreadAttrSetPriority(thread_id, parameter.priority);
		}
	}

	public ThreadState ThreadState
	{
		get
		{
			lock (lock_obj)
			{
				return _ThreadState;
			}
		}
		protected set
		{
			lock (lock_obj)
			{
				_ThreadState = value;
			}
		}
	}

	public Exception exception { get; protected set; }

	public AvsThread(ThreadStart _start, uint _sz_stack, AvsTypes.AVS_THREAD_PRIORITY _priority)
	{
		parameter.start = _start;
		parameter.sz_stack = _sz_stack;
		parameter.priority = _priority;
	}

	~AvsThread()
	{
		if (thread_id > 0)
		{
			Avs.AvsThreadDestroy(thread_id);
			thread_id = 0;
		}
	}

	public void Abort()
	{
		if (ThreadState == ThreadState.Running)
		{
			Avs.AvsThreadDestroy(thread_id);
			thread_id = 0;
			ThreadState = ThreadState.Aborted;
		}
	}

	public void Destroy()
	{
		if (thread_id > 0)
		{
			Avs.AvsThreadDestroy(thread_id);
			thread_id = 0;
		}
	}

	public bool Join()
	{
		Avs.AvsThreadJoin(thread_id);
		return true;
	}

	public void Start()
	{
		lock (lock_obj)
		{
			if (ThreadState != ThreadState.Unstarted)
			{
				throw new ThreadStateException("Thread has already been started.");
			}
			do
			{
				counter++;
				id = counter;
			}
			while (invoke_list.ContainsKey(id));
			invoke_list.Add(id, this);
			ThreadState = ThreadState.Running;
			thread_id = Avs.AvsThreadCreate(ThreadFunc, parameter.sz_stack, parameter.priority, id.ToString());
			if (thread_id > 0)
			{
				return;
			}
			ThreadState = ThreadState.Stopped;
			ErrorCode errorCode = thread_id;
			ErrorCode errorCode2 = errorCode;
			exception = new ThreadStateException("AvsThreadCreate error : " + errorCode2.ToString());
			throw exception;
		}
	}

	private static void ThreadFunc(string param)
	{
		AvsThread avsThread = null;
		try
		{
			int key = int.Parse(param);
			lock (lock_obj)
			{
				if (invoke_list.ContainsKey(key))
				{
					avsThread = invoke_list[key];
					invoke_list.Remove(key);
				}
			}
			avsThread?.parameter.start();
		}
		catch (Exception ex)
		{
			Exception e = (avsThread.exception = ex);
			Logger.LogException(e);
		}
		finally
		{
			if (avsThread != null)
			{
				avsThread.ThreadState = ThreadState.Stopped;
			}
		}
		Avs.AvsThreadExit(IntPtr.Zero);
	}
}
