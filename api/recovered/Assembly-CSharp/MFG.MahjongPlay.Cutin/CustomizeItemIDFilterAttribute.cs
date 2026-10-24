using System;
using MFG.Types.Customize.CustomizeItem;
using UnityEngine;

namespace MFG.MahjongPlay.Cutin;

[AttributeUsage(AttributeTargets.Field)]
public class CustomizeItemIDFilterAttribute : PropertyAttribute
{
	public Category Category;

	public CustomizeItemIDFilterAttribute(Category category)
	{
		Category = category;
	}
}
