using System;

namespace KAMUNITY.THREADING;

public class ThreadWithParamCallback<TParam, TResult>
{
	private Func<TParam, TResult> m_ThreadFunc;

	private Action<TResult> m_ResultFunc;

	private TParam m_Param;

	public ThreadWithParamCallback(Func<TParam, TResult> threadFunc, Action<TResult> resultFunc, TParam param)
	{
		m_ThreadFunc = threadFunc;
		m_ResultFunc = resultFunc;
		m_Param = param;
	}

	public void ThreadStart()
	{
		m_ResultFunc(m_ThreadFunc(m_Param));
	}
}
