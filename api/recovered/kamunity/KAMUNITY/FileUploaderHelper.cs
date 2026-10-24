using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace KAMUNITY;

public class FileUploaderHelper
{
	private enum STEP
	{
		AddFile,
		DeclareUpload,
		DeclareUpload_End,
		UploadFile,
		UploadFile_End,
		CommitUpload,
		End,
		Error
	}

	private List<UploaderFileData> m_FileList = new List<UploaderFileData>();

	public List<XrpcModuleUploaderTypes.DeclareUploadRequest_Content> m_ContentList = new List<XrpcModuleUploaderTypes.DeclareUploadRequest_Content>();

	private XrpcModuleUploaderTypes.DeclareUploadResponse m_UploaderDeclareUploadResponse;

	private Action m_Callback;

	private AvsEa3XrpcTypes.XRPC_STATUS_DATA m_XrpcStatus;

	private STEP m_Step;

	public bool isError => m_Step == STEP.Error;

	public AvsEa3XrpcTypes.XRPC_STATUS_DATA xrpcStatus => m_XrpcStatus;

	public XrpcModuleUploaderTypes.DeclareUploadResponse declareUploadResponse => m_UploaderDeclareUploadResponse;

	public void AddFileData(UploaderFileData fileData)
	{
		m_FileList.Add(fileData);
		m_ContentList.Add(fileData.content);
	}

	public void DeclareUpload(string bindedUser, Action cb)
	{
		m_Step = STEP.DeclareUpload;
		m_Callback = cb;
		XrpcModuleUploader.XrpcMethodDeclareUpload.Request(XrpcModuleUploaderTypes.ServiceCategoryId.Machine, bindedUser, m_ContentList, delegate(XrpcBase xrpc, XrpcModuleUploaderTypes.DeclareUploadResponse response, object cb_param)
		{
			m_XrpcStatus = xrpc.Status;
			if (!xrpc.Status.HasError())
			{
				m_UploaderDeclareUploadResponse = response;
				m_Step = STEP.DeclareUpload_End;
			}
			else
			{
				m_Step = STEP.Error;
			}
			CallCallback();
		}, null);
	}

	public void UploadFile(Action cb)
	{
		m_Step = STEP.UploadFile;
		m_Callback = cb;
		string text = "\r\n";
		string text2 = Utility.MakeMultiPartBoundaryCode();
		string text3 = "--" + text2;
		try
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(m_UploaderDeclareUploadResponse.uploadUrl);
			httpWebRequest.Method = "POST";
			httpWebRequest.Proxy = null;
			httpWebRequest.ContentType = "multipart/form-data; boundary=" + text2;
			httpWebRequest.Host = new Uri(m_UploaderDeclareUploadResponse.uploadUrl).Host;
			httpWebRequest.UserAgent = "KAMUNITY";
			httpWebRequest.KeepAlive = false;
			using (Stream stream = httpWebRequest.GetRequestStream())
			{
				Utility.WriteAsciiStringToStream(stream, text3 + text);
				Utility.WriteAsciiStringToStream(stream, "content-disposition: form-data; name=\"accessKey\"" + text + text);
				Utility.WriteAsciiStringToStream(stream, m_UploaderDeclareUploadResponse.accessKey);
				for (int i = 0; i < m_ContentList.Count; i++)
				{
					Utility.WriteAsciiStringToStream(stream, text + text3 + text);
					Utility.WriteAsciiStringToStream(stream, "content-disposition: form-data; name=\"contentBody\"; filename=\"" + m_ContentList[i].relativePath + "\"" + text);
					Utility.WriteAsciiStringToStream(stream, "Content-Type: " + m_ContentList[i].contentType + text + text);
					stream.Write(m_FileList[i].data, 0, m_FileList[i].data.Length);
				}
				Utility.WriteAsciiStringToStream(stream, text + text3 + "--" + text);
			}
			using HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			if (int.Parse(httpWebResponse.Headers.Get("X-Eamuse-Uploader-Status")) != 200)
			{
				m_Step = STEP.Error;
			}
			else
			{
				m_Step = STEP.UploadFile_End;
			}
		}
		catch (WebException e)
		{
			Logger.LogException(e);
			m_Step = STEP.Error;
		}
		catch (Exception e2)
		{
			Logger.LogException(e2);
			m_Step = STEP.Error;
		}
		CallCallback();
	}

	public void CommitUpload(Action cb)
	{
		m_Step = STEP.CommitUpload;
		m_Callback = cb;
		XrpcModuleUploader.XrpcMethodCommitUpload.Request(m_UploaderDeclareUploadResponse.arrangeNum, delegate(XrpcBase xrpc, object cb_param)
		{
			m_XrpcStatus = xrpc.Status;
			if (xrpc.Status.HasError())
			{
				m_Step = STEP.Error;
			}
			else
			{
				m_Step = STEP.End;
			}
			CallCallback();
		}, null);
	}

	public IEnumerator CoExecute(string bindedUser)
	{
		bool bEnd = false;
		DeclareUpload(bindedUser, delegate
		{
			bEnd = true;
		});
		yield return new WaitWhile(() => !bEnd);
		if (isError)
		{
			yield break;
		}
		bEnd = false;
		UploadFile(delegate
		{
			bEnd = true;
		});
		yield return new WaitWhile(() => !bEnd);
		if (!isError)
		{
			bEnd = false;
			CommitUpload(delegate
			{
				bEnd = true;
			});
			yield return new WaitWhile(() => !bEnd);
		}
	}

	private void CallCallback()
	{
		if (Utility.IsSafeDelegate(m_Callback))
		{
			try
			{
				m_Callback();
			}
			catch (Exception e)
			{
				Logger.LogException(e);
			}
		}
		m_Callback = null;
	}
}
