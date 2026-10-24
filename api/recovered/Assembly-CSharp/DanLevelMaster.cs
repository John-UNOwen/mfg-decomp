using System.Collections.Generic;
using MFG.GameData;
using MFG.Types;
using UnityEngine;

public class DanLevelMaster
{
	public class Item
	{
		public DanLevelType DanLevel;

		public string Name;

		public Dictionary<GainLossType, Dictionary<TakuType, List<int>>> GainLosValues;
	}

	private static readonly int ExpRequiredPerLevel = 100;

	private static List<Item> m_danItems = new List<Item>
	{
		new Item(),
		new Item
		{
			Name = "新人",
			DanLevel = DanLevelType.Newbie,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 60, 40, 20, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 60, 30, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 100, 50, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 100, 30, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "９級",
			DanLevel = DanLevelType.KYU9,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 45, 25, 10, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 45, 15, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 75, 35, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 80, 20, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "８級",
			DanLevel = DanLevelType.KYU8,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 45, 25, 10, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 45, 15, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 75, 35, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 80, 20, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "７級",
			DanLevel = DanLevelType.KYU7,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 45, 25, 10, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 45, 15, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 75, 35, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 80, 20, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "６級",
			DanLevel = DanLevelType.KYU6,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 35, 18, 7, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 35, 10, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 63, 27, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 69, 16, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "５級",
			DanLevel = DanLevelType.KYU5,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 35, 18, 7, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 35, 10, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 63, 27, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 69, 16, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "４級",
			DanLevel = DanLevelType.KYU4,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 35, 18, 7, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 35, 10, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 63, 27, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 69, 16, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "３級",
			DanLevel = DanLevelType.KYU3,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 25, 12, 3, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 25, 5, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 50, 20, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 58, 12, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "２級",
			DanLevel = DanLevelType.KYU2,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 25, 12, 3, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 25, 5, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 50, 20, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 58, 12, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "１級",
			DanLevel = DanLevelType.KYU1,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 25, 12, 3, 0 }
						},
						{
							TakuType.Sanma,
							new List<int> { 25, 5, 0, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 50, 20, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 58, 12, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "初段",
			DanLevel = DanLevelType.DAN1,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 17, 7, 1, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 17, 2, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 39, 15, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 49, 9, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "二段",
			DanLevel = DanLevelType.DAN2,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 17, 7, 1, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 17, 2, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 39, 15, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 49, 9, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "三段",
			DanLevel = DanLevelType.DAN3,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 17, 7, 1, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 17, 2, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 39, 15, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 49, 9, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "四段",
			DanLevel = DanLevelType.DAN4,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 17, 7, 1, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 17, 2, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 39, 15, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 49, 9, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "五段",
			DanLevel = DanLevelType.DAN5,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 12, 4, 1, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 12, 1, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 34, 12, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 46, 6, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "六段",
			DanLevel = DanLevelType.DAN6,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 12, 4, 1, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 12, 1, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 34, 12, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 46, 6, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "七段",
			DanLevel = DanLevelType.DAN7,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 12, 4, 1, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 12, 1, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 34, 12, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 46, 6, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "八段",
			DanLevel = DanLevelType.DAN8,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 7, 2, 0, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 7, 0, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 30, 8, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 43, 3, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "九段",
			DanLevel = DanLevelType.DAN9,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 7, 2, 0, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 7, 0, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 30, 8, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 43, 3, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "十段",
			DanLevel = DanLevelType.DAN10,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 7, 2, 0, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 7, 0, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 30, 8, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 43, 3, -40, 0 }
						}
					}
				}
			}
		}
	};

	private static List<Item> _janseiItems = new List<Item>
	{
		new Item
		{
			Name = "雀星Lv.",
			DanLevel = DanLevelType.JANSEI1,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, 0, -1 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -1, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 6, 0, -30 }
						},
						{
							TakuType.Sanma,
							new List<int> { 42, 1, -40, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★Lv.",
			DanLevel = (DanLevelType)120,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -2 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -2, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 6, -4, -31 }
						},
						{
							TakuType.Sanma,
							new List<int> { 32, 1, -31, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★★Lv.",
			DanLevel = (DanLevelType)219,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -3 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -3, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 6, -4, -32 }
						},
						{
							TakuType.Sanma,
							new List<int> { 32, 1, -32, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★★★Lv.",
			DanLevel = (DanLevelType)318,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -4 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -4, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 6, -4, -33 }
						},
						{
							TakuType.Sanma,
							new List<int> { 32, 1, -33, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★★★★Lv.",
			DanLevel = (DanLevelType)417,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -5 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -5, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 6, -4, -34 }
						},
						{
							TakuType.Sanma,
							new List<int> { 32, 1, -34, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★★★★★Lv.",
			DanLevel = (DanLevelType)516,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 6, -4, -35 }
						},
						{
							TakuType.Sanma,
							new List<int> { 32, 1, -35, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×6Lv.",
			DanLevel = (DanLevelType)615,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 5, -4, -36 }
						},
						{
							TakuType.Sanma,
							new List<int> { 28, 0, -32, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×7Lv.",
			DanLevel = (DanLevelType)714,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 5, -4, -37 }
						},
						{
							TakuType.Sanma,
							new List<int> { 28, 0, -33, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×8Lv.",
			DanLevel = (DanLevelType)813,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 5, -4, -38 }
						},
						{
							TakuType.Sanma,
							new List<int> { 28, 0, -34, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×9Lv.",
			DanLevel = (DanLevelType)912,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 5, -4, -39 }
						},
						{
							TakuType.Sanma,
							new List<int> { 28, 0, -35, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×10Lv.",
			DanLevel = (DanLevelType)1011,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 28, 5, -4, -40 }
						},
						{
							TakuType.Sanma,
							new List<int> { 28, 0, -36, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×11Lv.",
			DanLevel = (DanLevelType)1110,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 27, 5, -4, -41 }
						},
						{
							TakuType.Sanma,
							new List<int> { 27, 0, -36, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×12Lv.",
			DanLevel = (DanLevelType)1209,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 27, 5, -4, -42 }
						},
						{
							TakuType.Sanma,
							new List<int> { 27, 0, -37, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×13Lv.",
			DanLevel = (DanLevelType)1308,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 27, 5, -4, -43 }
						},
						{
							TakuType.Sanma,
							new List<int> { 27, 0, -38, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×14Lv.",
			DanLevel = (DanLevelType)1407,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 27, 5, -4, -44 }
						},
						{
							TakuType.Sanma,
							new List<int> { 27, 0, -39, 0 }
						}
					}
				}
			}
		},
		new Item
		{
			Name = "雀星★×15Lv.",
			DanLevel = (DanLevelType)1506,
			GainLosValues = new Dictionary<GainLossType, Dictionary<TakuType, List<int>>>
			{
				{
					GainLossType.Steadily,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 4, 1, -1, -6 }
						},
						{
							TakuType.Sanma,
							new List<int> { 4, 0, -6, 0 }
						}
					}
				},
				{
					GainLossType.Risky,
					new Dictionary<TakuType, List<int>>
					{
						{
							TakuType.Tonpu,
							new List<int> { 27, 5, -4, -45 }
						},
						{
							TakuType.Sanma,
							new List<int> { 27, 0, -40, 0 }
						}
					}
				}
			}
		}
	};

	public static int GetGetAcquiredFightStar(TakuType takuType, DanLevelType absoluteDanLevel, GainLossType gainLossType, Rank rank, bool isKiken = false)
	{
		bool num = gainLossType == GainLossType.SuperRisky;
		int num2 = ((!num) ? 1 : 2);
		if (num)
		{
			gainLossType = GainLossType.Risky;
		}
		if (takuType == TakuType.Nima || takuType == TakuType.Hanchan)
		{
			return 0;
		}
		if (isKiken && gainLossType == GainLossType.Steadily)
		{
			gainLossType = GainLossType.Risky;
		}
		if (absoluteDanLevel == DanLevelType.None)
		{
			Debug.LogWarning($"DanLevelTypeがnoneです {absoluteDanLevel}");
			absoluteDanLevel = DanLevelType.Newbie;
		}
		if (absoluteDanLevel <= DanLevelType.DAN10)
		{
			return m_danItems[(int)absoluteDanLevel].GainLosValues[gainLossType][takuType][(int)rank] * num2;
		}
		int item = GetDanLevelTypeWithReincarnation(absoluteDanLevel).reincarnationCount;
		return _janseiItems[item].GainLosValues[gainLossType][takuType][(int)rank] * num2;
	}

	public static int GetFightStarRequiredForNextDanLevel(DanLevelType absoluteDanLevel)
	{
		if (absoluteDanLevel < DanLevelType.Newbie || absoluteDanLevel > DanLevelType.Max)
		{
			return 0;
		}
		int totalFightStarRequired = GetTotalFightStarRequired(absoluteDanLevel);
		int totalFightStarRequired2 = GetTotalFightStarRequired(absoluteDanLevel + 1);
		if (totalFightStarRequired2 == 0)
		{
			return 0;
		}
		return totalFightStarRequired2 - totalFightStarRequired;
	}

	public static int GetTotalFightStarRequired(DanLevelType absoluteDanLevel)
	{
		if (absoluteDanLevel < DanLevelType.None || absoluteDanLevel > DanLevelType.Max)
		{
			return 0;
		}
		if (absoluteDanLevel == DanLevelType.None || absoluteDanLevel == DanLevelType.Newbie)
		{
			return 0;
		}
		return (int)(absoluteDanLevel - 1) * ExpRequiredPerLevel;
	}

	public static (DanLevelType currentDanLevel, int minFightStarAtCurrentDanLevel, int maxFightStarAtCurrentDanLevel) GetCurrentDanLevelAndFightStarBorder(int totalFightStar)
	{
		DanLevelType danLevelType = DanLevelType.Newbie;
		if (totalFightStar >= 0)
		{
			danLevelType = (DanLevelType)(totalFightStar / ExpRequiredPerLevel + 1);
		}
		if (danLevelType > DanLevelType.Max)
		{
			danLevelType = DanLevelType.Max;
		}
		return (currentDanLevel: danLevelType, minFightStarAtCurrentDanLevel: GetTotalFightStarRequired(danLevelType), maxFightStarAtCurrentDanLevel: GetTotalFightStarRequired(danLevelType + 1));
	}

	public static (int reincarnationCount, DanLevelType danLevelTypeWithReincarnation) GetDanLevelTypeWithReincarnation(DanLevelType absoluteDanLevelType)
	{
		if (absoluteDanLevelType <= DanLevelType.JANSEI99)
		{
			return (reincarnationCount: 0, danLevelTypeWithReincarnation: absoluteDanLevelType);
		}
		int num = (int)(absoluteDanLevelType - 21) / 99;
		DanLevelType item = absoluteDanLevelType - num * 99;
		return (reincarnationCount: num, danLevelTypeWithReincarnation: item);
	}

	public static string DanLevelType_ToString(DanLevelType absoluteDanLevelType)
	{
		if (absoluteDanLevelType <= DanLevelType.DAN10)
		{
			return m_danItems[(int)absoluteDanLevelType].Name;
		}
		if (absoluteDanLevelType <= DanLevelType.JANSEI99)
		{
			return $"雀星Lv.{(int)(absoluteDanLevelType - 21 + 1)}";
		}
		var (num, danLevelType) = GetDanLevelTypeWithReincarnation(absoluteDanLevelType);
		return $"雀星★{num} Lv.{(int)(danLevelType - 21 + 1)}";
	}

	public static string DanLevelMissionDispName(DanLevelType absoluteDanLevelType)
	{
		if (absoluteDanLevelType <= DanLevelType.DAN10)
		{
			return m_danItems[(int)absoluteDanLevelType].Name;
		}
		if (absoluteDanLevelType <= DanLevelType.JANSEI99)
		{
			return $"雀星Lv.{(int)(absoluteDanLevelType - 21 + 1)}";
		}
		var (reincarnationCount, danLevelType) = GetDanLevelTypeWithReincarnation(absoluteDanLevelType);
		return $"{_janseiItems.Find((Item x) => x.DanLevel == (DanLevelType)(21 + 99 * reincarnationCount)).Name}{(int)(danLevelType - 21 + 1)}";
	}

	private static int GetFightStarCorrectionValueAfterJansei(int reincarnationCount, TakuType takuType, GainLossType gainLossType, Rank rank)
	{
		switch (gainLossType)
		{
		case GainLossType.Risky:
			switch (takuType)
			{
			case TakuType.Tonpu:
				if (rank != Rank.Rank4)
				{
					break;
				}
				return -1 * reincarnationCount;
			case TakuType.Sanma:
				if (rank != Rank.Rank3)
				{
					break;
				}
				return -1 * reincarnationCount;
			}
			break;
		case GainLossType.Steadily:
			switch (takuType)
			{
			case TakuType.Tonpu:
				if (rank != Rank.Rank4)
				{
					break;
				}
				return -1 * reincarnationCount;
			case TakuType.Sanma:
				if (rank != Rank.Rank3)
				{
					break;
				}
				return -1 * reincarnationCount;
			}
			break;
		}
		return 0;
	}
}
