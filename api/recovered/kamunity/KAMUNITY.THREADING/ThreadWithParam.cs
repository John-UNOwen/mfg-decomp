using System;

namespace KAMUNITY.THREADING;

public class ThreadWithParam<TParam>
{
	private Action<TParam> m_ThreadFunc;

	private TParam m_Param;

	public ThreadWithParam(Action<TParam> threadFunc, TParam param)
	{
		m_ThreadFunc = threadFunc;
		m_Param = param;
	}

	public void ThreadStart()
	{
		m_ThreadFunc(m_Param);
	}
}
