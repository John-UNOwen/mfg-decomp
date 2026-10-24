using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DebugCommandWindow : MonoBehaviour
{
	[SerializeField]
	private bool m_debugMenuOpen;

	[SerializeField]
	private Button _foldoutButton;

	[SerializeField]
	private GameObject _panelPrefab;

	[SerializeField]
	private GameObject _folderPrefab;

	[SerializeField]
	private GameObject _itemParent;

	[SerializeField]
	private ScrollRect _scrollRect;

	private bool _isAllOpen = true;

	private List<DebugFlagUtility.FoldoutGroup> _foldoutGroups = new List<DebugFlagUtility.FoldoutGroup>();

	private bool _initialized;

	public void ToggleActive()
	{
		Activate(!m_debugMenuOpen);
	}

	public void Activate(bool active)
	{
		m_debugMenuOpen = active;
		base.gameObject.SetActive(m_debugMenuOpen);
		if (m_debugMenuOpen)
		{
			Cursor.visible = active;
		}
		Reflesh();
	}

	private void Reflesh()
	{
		if (!m_debugMenuOpen)
		{
			return;
		}
		foreach (DebugFlagUtility.FoldoutGroup foldoutGroup in _foldoutGroups)
		{
			foldoutGroup.Reflesh();
		}
	}

	public void Initialize()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		_foldoutGroups = DebugFlagUtility.GenerateFoldoutGroups(_isAllOpen);
		_foldoutButton.GetComponentInChildren<TextMeshProUGUI>().text = (_isAllOpen ? "全て折りたたみ" : "全て展開");
		_foldoutButton.OnClickAsObservable().Subscribe(delegate
		{
			_isAllOpen = !_isAllOpen;
			_foldoutButton.GetComponentInChildren<TextMeshProUGUI>().text = (_isAllOpen ? "全て折りたたみ" : "全て展開");
			FoldoutGroup_SetAllOpen(_isAllOpen);
			Reflesh();
		}).AddTo(this);
		foreach (Transform item in _itemParent.transform)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (DebugFlagUtility.FoldoutGroup foldoutGroup in _foldoutGroups)
		{
			foldoutGroup.CreateView(_panelPrefab, _folderPrefab, _itemParent);
		}
	}

	private void FoldoutGroup_SetAllOpen(bool isOpen)
	{
		foreach (DebugFlagUtility.FoldoutGroup foldoutGroup in _foldoutGroups)
		{
			foldoutGroup.IsOpen = isOpen;
		}
	}
}
