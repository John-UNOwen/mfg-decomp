using System;
using System.Collections.Generic;
using System.Linq;
using TestModeSkeleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopNameSetting : MenuItemsBase
{
	[Serializable]
	public class ValueTexts
	{
		public Text setTimeHour;

		public Text setTimeMin;

		public Text setTimeSec;

		public Text initialized;

		public Text setDate;
	}

	public ValueTexts texts;

	public GameObject m_menu;

	public GameObject m_contents;

	public GameObject m_shopInput;

	private string m_shopName = "";

	private List<string>[] m_fontList;

	private int m_contentsIndex;

	private int m_oldContentsIndex = -1;

	private bool m_isSelectContents;

	private int CONTENTS_FONT_X_COUNT = 25;

	private string[] strMenu = new string[48]
	{
		"あ", "い", "う", "え", "お", "か", "き", "く", "け", "こ",
		"さ", "し", "す", "せ", "そ", "た", "ち", "つ", "て", "と",
		"な", "に", "ぬ", "ね", "の", "は", "ひ", "ふ", "へ", "ほ",
		"ま", "み", "む", "め", "も", "や", "ゆ", "よ", "ら", "り",
		"る", "れ", "ろ", "わ", "ゐ", "ゑ", "を", "ん"
	};

	private string[] strRightOther = new string[48]
	{
		"あぁアァ", "いぃイィ", "うぅウヴゥ", "えぇエェ", "おぉオォ", "かがカガヵ", "きぎキギ", "くぐクグ", "けげケゲヶ", "こごコゴ",
		"さざサザ", "しじシジ", "すずスズ", "せぜセゼ", "そぞソゾ", "ただタダ", "ちぢチヂ", "つづっツヅッ", "てでテデ", "とどトド",
		"なナ", "にニ", "ぬヌ", "ねネ", "のノ", "はばぱハバパ", "ひびぴヒビピ", "ふぶぷフブプ", "へべぺヘベペ", "ほぼぽホボポ",
		"まマ", "みミ", "むム", "めメ", "もモ", "やゃヤャ", "ゆゅユュ", "よょヨョ", "らラ", "りリ",
		"るル", "れレ", "ろロ", "わワ", "ゐヰ", "ゑヱ", "をヲ", "んン"
	};

	private string[] firstChr = new string[48]
	{
		"亜", "以", "右", "荏", "於", "下", "企", "九", "卦", "乎",
		"些", "仕", "諏", "世", "噌", "他", "値", "津", "亭", "兎",
		"奈", "二", "濡", "禰", "乃", "巴", "匪", "不", "丙", "保",
		"摩", "味", "務", "冥", "摸", "也", "愉", "予", "羅", "利",
		"瑠", "令", "呂", "倭", "  ", "  ", "  ", "  "
	};

	protected override void MenuSceneStart()
	{
		if ((bool)titleText)
		{
			titleText.text = "店舗名設定";
		}
		Scene sceneByName = SceneManager.GetSceneByName("NetworkOptions");
		if (sceneByName.IsValid())
		{
			m_shopName = sceneByName.GetRootGameObjects().First().GetComponent<NetworkOptions>()
				.GetShopName();
		}
		m_fontList = new List<string>[m_menu.transform.childCount];
		int num = 0;
		foreach (Transform item in m_menu.transform)
		{
			m_fontList[num] = new List<string>();
			if (num < strMenu.Length)
			{
				item.GetComponent<Text>().text = strMenu[num];
				MenuElement menuElement = new MenuElement();
				menuElement.EventType = EVENT_TYPE.EVENT;
				menuElement.Event = "Font";
				menuElement.MenuText = item.GetComponent<Text>();
				menuElement.InfoState = TestModeHelp.HelpType.UDLRSelectExecute;
				m_MenuElements.Insert(num, menuElement);
				SetFontList(num);
			}
			else
			{
				string @event = m_MenuElements[num].Event;
				if (!(@event == "Alphabet"))
				{
					if (@event == "Symbol")
					{
						SetSymbolList(m_fontList[num]);
					}
				}
				else
				{
					SetAlphabetList(m_fontList[num]);
				}
			}
			num++;
		}
		m_currentCursor = 0;
		SetContents(m_currentCursor);
	}

	protected override void MenuSceneUpdate(bool menuPanelActiveInHierarchy)
	{
		if (m_contentsIndex != m_oldContentsIndex)
		{
			if (m_oldContentsIndex >= 0 && m_oldContentsIndex < m_contents.transform.childCount)
			{
				m_contents.transform.GetChild(m_oldContentsIndex).GetComponent<Text>().color = Color.white;
				m_contents.transform.GetChild(m_contentsIndex).GetComponent<Text>().color = Color.red;
			}
			m_oldContentsIndex = m_contentsIndex;
		}
	}

	protected override bool OnCursorElement(MenuElement element)
	{
		SetContents(m_currentCursor);
		SetShopName();
		return base.OnCursorElement(element);
	}

	protected override void TriggerAction(TestModeInput.Key key)
	{
		if (m_isSelectContents)
		{
			switch (key)
			{
			case TestModeInput.Key.Up:
				m_contentsIndex = GetNextContentsCursor(isBack: true, x_skip: true);
				break;
			case TestModeInput.Key.Down:
				m_contentsIndex = GetNextContentsCursor(isBack: false, x_skip: true);
				break;
			case TestModeInput.Key.Left:
				m_contentsIndex = GetNextContentsCursor(isBack: true);
				break;
			case TestModeInput.Key.Right:
				m_contentsIndex = GetNextContentsCursor();
				break;
			case TestModeInput.Key.Test:
				m_contentsIndex = GetNextContentsCursor();
				break;
			default:
				base.TriggerAction(key);
				break;
			}
		}
		else if (m_MenuElements[m_currentCursor].Event == "Font")
		{
			switch (key)
			{
			case TestModeInput.Key.Up:
			case TestModeInput.Key.Down:
			{
				if (key == TestModeInput.Key.Up && m_currentCursor < 5)
				{
					m_currentCursor = m_MenuElements.Count - 1;
					break;
				}
				if (key == TestModeInput.Key.Down && m_currentCursor + 5 > strMenu.Length)
				{
					m_currentCursor = strMenu.Length;
					break;
				}
				bool isBack2 = key == TestModeInput.Key.Up;
				int skipNum = GetSkipNum(isBack2);
				m_currentCursor = GetNextCursor(m_MenuElements, m_currentCursor, isBack2, skipNum);
				break;
			}
			case TestModeInput.Key.Left:
			case TestModeInput.Key.Right:
			{
				bool isBack = key == TestModeInput.Key.Left;
				m_currentCursor = GetNextCursor(m_MenuElements, m_currentCursor, isBack);
				break;
			}
			default:
				base.TriggerAction(key);
				break;
			}
		}
		else
		{
			base.TriggerAction(key);
		}
	}

	protected override void ChosenElement(MenuElement element, CALL_TYPE c_type, TestModeInput.Key key)
	{
		if ((key != TestModeInput.Key.Enter && key != TestModeInput.Key.Service) || c_type != 0)
		{
			return;
		}
		switch (element.Event)
		{
		case "Alphabet":
		case "Symbol":
		case "Font":
			if (m_isSelectContents)
			{
				AddShopName(m_fontList[m_currentCursor][m_contentsIndex]);
				m_contents.transform.GetChild(m_contentsIndex).GetComponent<Text>().color = Color.white;
				m_contentsIndex = 0;
				m_oldContentsIndex = -1;
				SetShopName();
				base.SetKeyInfoState = m_MenuElements[m_currentCursor].InfoState;
			}
			else
			{
				m_oldContentsIndex = m_contentsIndex;
				m_contents.transform.GetChild(m_contentsIndex).GetComponent<Text>().color = Color.red;
				base.SetKeyInfoState = TestModeHelp.HelpType.UDLRSelectExecute;
			}
			m_isSelectContents = !m_isSelectContents;
			break;
		case "Space":
			AddShopName("\u3000");
			SetShopName();
			break;
		case "Delete":
			if (m_shopName.Length > 0)
			{
				m_shopName = m_shopName.Remove(m_shopName.Length - 1);
			}
			SetShopName();
			break;
		}
	}

	private void SetFontList(int index)
	{
		string text = strRightOther[index];
		foreach (char c in text)
		{
			m_fontList[index].Add(c.ToString());
		}
		bool flag = false;
		text = "亜唖娃阿哀愛挨姶逢葵茜穐悪握渥旭葦芦鯵梓圧斡扱宛姐虻飴絢綾鮎或粟袷安庵按暗案闇鞍杏以伊位依偉囲夷委威尉惟意慰易椅為畏異移維緯胃萎衣謂違遺医井亥域育郁磯一壱溢逸稲茨芋鰯允印咽員因姻引飲淫胤蔭院陰隠韻吋右宇烏羽迂雨卯鵜窺丑碓臼渦嘘唄欝蔚鰻姥厩浦瓜閏噂云運雲荏餌叡営嬰影映曳栄永泳洩瑛盈穎頴英衛詠鋭液疫益駅悦謁越閲榎厭円園堰奄宴延怨掩援沿演炎焔煙燕猿縁艶苑薗遠鉛鴛塩於汚甥凹央奥往応押旺横欧殴王翁襖鴬鴎黄岡沖荻億屋憶臆桶牡乙俺卸恩温穏音下化仮何伽価佳加可嘉夏嫁家寡科暇果架歌河火珂禍禾稼箇花苛茄荷華菓蝦課嘩貨迦過霞蚊俄峨我牙画臥芽蛾賀雅餓駕介会解回塊壊廻快怪悔恢懐戒拐改魁晦械海灰界皆絵芥蟹開階貝凱劾外咳害崖慨概涯碍蓋街該鎧骸浬馨蛙垣柿蛎鈎劃嚇各廓拡撹格核殻獲確穫覚角赫較郭閣隔革学岳楽額顎掛笠樫橿梶鰍潟割喝恰括活渇滑葛褐轄且鰹叶椛樺鞄株兜竃蒲釜鎌噛鴨栢茅萱粥刈苅瓦乾侃冠寒刊勘勧巻喚堪姦完官寛干幹患感慣憾換敢柑桓棺款歓汗漢澗潅環甘監看竿管簡緩缶翰肝艦莞観諌貫還鑑間閑関陥韓館舘丸含岸巌玩癌眼岩翫贋雁頑顔願企伎危喜器基奇嬉寄岐希幾忌揮机旗既期棋棄機帰毅気汽畿祈季稀紀徽規記貴起軌輝飢騎鬼亀偽儀妓宜戯技擬欺犠疑祇義蟻誼議掬菊鞠吉吃喫桔橘詰砧杵黍却客脚虐逆丘久仇休及吸宮弓急救朽求汲泣灸球究窮笈級糾給旧牛去居巨拒拠挙渠虚許距鋸漁禦魚亨享京供侠僑兇競共凶協匡卿叫喬境峡強彊怯恐恭挟教橋況狂狭矯胸脅興蕎郷鏡響饗驚仰凝尭暁業局曲極玉桐粁僅勤均巾錦斤欣欽琴禁禽筋緊芹菌衿襟謹近金吟銀九倶句区狗玖矩苦躯駆駈駒具愚虞喰空偶寓遇隅串櫛釧屑屈掘窟沓靴轡窪熊隈粂栗繰桑鍬勲君薫訓群軍郡卦袈祁係傾刑兄啓圭珪型契形径恵慶慧憩掲携敬景桂渓畦稽系経継繋罫茎荊蛍計詣警軽頚鶏芸迎鯨劇戟撃激隙桁傑欠決潔穴結血訣月件倹倦健兼券剣喧圏堅嫌建憲懸拳捲検権牽犬献研硯絹県肩見謙賢軒遣鍵険顕験鹸元原厳幻弦減源玄現絃舷言諺限乎個古呼固姑孤己庫弧戸故枯湖狐糊袴股胡菰虎誇跨鈷雇顧鼓五互伍午呉吾娯後御悟梧檎瑚碁語誤護醐乞鯉交佼侯候倖光公功効勾厚口向后喉坑垢好孔孝宏工巧巷幸広庚康弘恒慌抗拘控攻昂晃更杭校梗構江洪浩港溝甲皇硬稿糠紅紘絞綱耕考肯肱腔膏航荒行衡講貢購郊酵鉱砿鋼閤降項香高鴻剛劫号合壕拷濠豪轟麹克刻告国穀酷鵠黒獄漉腰甑忽惚骨狛込此頃今困坤墾婚恨懇昏昆根梱混痕紺艮魂些佐叉唆嵯左差査沙瑳砂詐鎖裟坐座挫債催再最哉塞妻宰彩才採栽歳済災采犀砕砦祭斎細菜裁載際剤在材罪財冴坂阪堺榊肴咲崎埼碕鷺作削咋搾昨朔柵窄策索錯桜鮭笹匙冊刷察拶撮擦札殺薩雑皐鯖捌錆鮫皿晒三傘参山惨撒散桟燦珊産算纂蚕讃賛酸餐斬暫残仕仔伺使刺司史嗣四士始姉姿子屍市師志思指支孜斯施旨枝止死氏獅祉私糸紙紫肢脂至視詞詩試誌諮資賜雌飼歯事似侍児字寺慈持時次滋治爾璽痔磁示而耳自蒔辞汐鹿式識鴫竺軸宍雫七叱執失嫉室悉湿漆疾質実蔀篠偲柴芝屡蕊縞舎写射捨赦斜煮社紗者謝車遮蛇邪借勺尺杓灼爵酌釈錫若寂弱惹主取守手朱殊狩珠種腫趣酒首儒受呪寿授樹綬需囚収周宗就州修愁拾洲秀秋終繍習臭舟蒐衆襲讐蹴輯週酋酬集醜什住充十従戎柔汁渋獣縦重銃叔夙宿淑祝縮粛塾熟出術述俊峻春瞬竣舜駿准循旬楯殉淳準潤盾純巡遵醇順処初所暑曙渚庶緒署書薯藷諸助叙女序徐恕鋤除傷償勝匠升召哨商唱嘗奨妾娼宵将小少尚庄床廠彰承抄招掌捷昇昌昭晶松梢樟樵沼消渉湘焼焦照症省硝礁祥称章笑粧紹肖菖蒋蕉衝裳訟証詔詳象賞醤鉦鍾鐘障鞘上丈丞乗冗剰城場壌嬢常情擾条杖浄状畳穣蒸譲醸錠嘱埴飾拭植殖燭織職色触食蝕辱尻伸信侵唇娠寝審心慎振新晋森榛浸深申疹真神秦紳臣芯薪親診身辛進針震人仁刃塵壬尋甚尽腎訊迅陣靭笥諏須酢図厨逗吹垂帥推水炊睡粋翠衰遂酔錐錘随瑞髄崇嵩数枢趨雛据杉椙菅頗雀裾澄摺寸世瀬畝是凄制勢姓征性成政整星晴棲栖正清牲生盛精聖声製西誠誓請逝醒青静斉税脆隻席惜戚斥昔析石積籍績脊責赤跡蹟碩切拙接摂折設窃節説雪絶舌蝉仙先千占宣専尖川戦扇撰栓栴泉浅洗染潜煎煽旋穿箭線繊羨腺舛船薦詮賎践選遷銭銑閃鮮前善漸然全禅繕膳糎噌塑岨措曾曽楚狙疏疎礎祖租粗素組蘇訴阻遡鼠僧創双叢倉喪壮奏爽宋層匝惣想捜掃挿掻操早曹巣槍槽漕燥争痩相窓糟総綜聡草荘葬蒼藻装走送遭鎗霜騒像増憎臓蔵贈造促側則即息捉束測足速俗属賊族続卒袖其揃存孫尊損村遜他多太汰詑唾堕妥惰打柁舵楕陀駄騨体堆対耐岱帯待怠態戴替泰滞胎腿苔袋貸退逮隊黛鯛代台大第醍題鷹滝瀧卓啄宅托択拓沢濯琢託鐸濁諾茸凧蛸只叩但達辰奪脱巽竪辿棚谷狸鱈樽誰丹単嘆坦担探旦歎淡湛炭短端箪綻耽胆蛋誕鍛団壇弾断暖檀段男談値知地弛恥智池痴稚置致蜘遅馳築畜竹筑蓄逐秩窒茶嫡着中仲宙忠抽昼柱注虫衷註酎鋳駐樗瀦猪苧著貯丁兆凋喋寵帖帳庁弔張彫徴懲挑暢朝潮牒町眺聴脹腸蝶調諜超跳銚長頂鳥勅捗直朕沈珍賃鎮陳津墜椎槌追鎚痛通塚栂掴槻佃漬柘辻蔦綴鍔椿潰坪壷嬬紬爪吊釣鶴亭低停偵剃貞呈堤定帝底庭廷弟悌抵挺提梯汀碇禎程締艇訂諦蹄逓邸鄭釘鼎泥摘擢敵滴的笛適鏑溺哲徹撤轍迭鉄典填天展店添纏甜貼転顛点伝殿澱田電兎吐堵塗妬屠徒斗杜渡登菟賭途都鍍砥砺努度土奴怒倒党冬凍刀唐塔塘套宕島嶋悼投搭東桃梼棟盗淘湯涛灯燈当痘祷等答筒糖統到董蕩藤討謄豆踏逃透鐙陶頭騰闘働動同堂導憧撞洞瞳童胴萄道銅峠鴇匿得徳涜特督禿篤毒独読栃橡凸突椴届鳶苫寅酉瀞噸屯惇敦沌豚遁頓呑曇鈍奈那内乍凪薙謎灘捺鍋楢馴縄畷南楠軟難汝二尼弐迩匂賑肉虹廿日乳入如尿韮任妊忍認濡禰祢寧葱猫熱年念捻撚燃粘乃廼之埜嚢悩濃納能脳膿農覗蚤巴把播覇杷波派琶破婆罵芭馬俳廃拝排敗杯盃牌背肺輩配倍培媒梅楳煤狽買売賠陪這蝿秤矧萩伯剥博拍柏泊白箔粕舶薄迫曝漠爆縛莫駁麦函箱硲箸肇筈櫨幡肌畑畠八鉢溌発醗髪伐罰抜筏閥鳩噺塙蛤隼伴判半反叛帆搬斑板氾汎版犯班畔繁般藩販範釆煩頒飯挽晩番盤磐蕃蛮匪卑否妃庇彼悲扉批披斐比泌疲皮碑秘緋罷肥被誹費避非飛樋簸備尾微枇毘琵眉美鼻柊稗匹疋髭彦膝菱肘弼必畢筆逼桧姫媛紐百謬俵彪標氷漂瓢票表評豹廟描病秒苗錨鋲蒜蛭鰭品彬斌浜瀕貧賓頻敏瓶不付埠夫婦富冨布府怖扶敷斧普浮父符腐膚芙譜負賦赴阜附侮撫武舞葡蕪部封楓風葺蕗伏副復幅服福腹複覆淵弗払沸仏物鮒分吻噴墳憤扮焚奮粉糞紛雰文聞丙併兵塀幣平弊柄並蔽閉陛米頁僻壁癖碧別瞥蔑箆偏変片篇編辺返遍便勉娩弁鞭保舗鋪圃捕歩甫補輔穂募墓慕戊暮母簿菩倣俸包呆報奉宝峰峯崩庖抱捧放方朋法泡烹砲縫胞芳萌蓬蜂褒訪豊邦鋒飽鳳鵬乏亡傍剖坊妨帽忘忙房暴望某棒冒紡肪膨謀貌貿鉾防吠頬北僕卜墨撲朴牧睦穆釦勃没殆堀幌奔本翻凡盆摩磨魔麻埋妹昧枚毎哩槙幕膜枕鮪柾鱒桝亦俣又抹末沫迄侭繭麿万慢満漫蔓味未魅巳箕岬密蜜湊蓑稔脈妙粍民眠務夢無牟矛霧鵡椋婿娘冥名命明盟迷銘鳴姪牝滅免棉綿緬面麺摸模茂妄孟毛猛盲網耗蒙儲木黙目杢勿餅尤戻籾貰問悶紋門匁也冶夜爺耶野弥矢厄役約薬訳躍靖柳薮鑓愉愈油癒諭輸唯佑優勇友宥幽悠憂揖有柚湧涌猶猷由祐裕誘遊邑郵雄融夕予余与誉輿預傭幼妖容庸揚揺擁曜楊様洋溶熔用窯羊耀葉蓉要謡踊遥陽養慾抑欲沃浴翌翼淀羅螺裸来莱頼雷洛絡落酪乱卵嵐欄濫藍蘭覧利吏履李梨理璃痢裏裡里離陸律率立葎掠略劉流溜琉留硫粒隆竜龍侶慮旅虜了亮僚両凌寮料梁涼猟療瞭稜糧良諒遼量陵領力緑倫厘林淋燐琳臨輪隣鱗麟瑠塁涙累類令伶例冷励嶺怜玲礼苓鈴隷零霊麗齢暦歴列劣烈裂廉恋憐漣煉簾練聯蓮連錬呂魯櫓炉賂路露労婁廊弄朗楼榔浪漏牢狼篭老聾蝋郎六麓禄肋録論倭和話歪賄脇惑枠鷲亙亘鰐詫藁蕨椀湾碗腕";
		for (int i = 0; i < text.Length; i++)
		{
			string text2 = text[i].ToString();
			if (index + 1 >= firstChr.Length || !(text2 == firstChr[index + 1]))
			{
				if (text2 == firstChr[index] || flag)
				{
					flag = true;
					m_fontList[index].Add(text2);
				}
				continue;
			}
			break;
		}
	}

	private void SetAlphabetList(List<string> list)
	{
		string text = "０１２３４５６７８９ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ";
		for (int i = 0; i < text.Length; i++)
		{
			list.Add(text[i].ToString());
		}
	}

	private void SetSymbolList(List<string> list)
	{
		string text = "、。，．・：；？！\u309b\u309c\u00b4\uff40\u00a8\uff3e\uffe3\uff3fヽヾゝゞ〃仝々〆〇ー―‐／＼～∥｜…‥‘’“”（）〔〕［］｛｝〈〉《》「」『』【】＋－±×÷＝≠＜＞≦≧∞∴♂♀°′″℃￥＄￠￡％＃＆＊＠§☆★○●◎◇◆□■△▲▽▼※〒→←↑↓〓";
		for (int i = 0; i < text.Length; i++)
		{
			list.Add(text[i].ToString());
		}
	}

	private void SetContents(int index)
	{
		int num = 0;
		foreach (Transform item in m_contents.transform)
		{
			if (num < m_fontList[index].Count)
			{
				item.gameObject.SetActive(value: true);
				if ((bool)item.GetComponent<Text>())
				{
					item.GetComponent<Text>().text = m_fontList[index][num];
				}
			}
			else
			{
				item.gameObject.SetActive(value: false);
			}
			num++;
		}
	}

	private void AddShopName(string add)
	{
		if (m_shopName.Length >= m_shopInput.transform.childCount)
		{
			m_shopName = m_shopName.Remove(m_shopName.Length - 1);
		}
		m_shopName += add;
	}

	private void SetShopName()
	{
		int num = 0;
		foreach (Transform item in m_shopInput.transform)
		{
			Text component = item.GetComponent<Text>();
			if (num < m_shopName.Length)
			{
				component.text = m_shopName[num].ToString();
			}
			else
			{
				component.text = "_";
			}
			component.color = ((num == m_shopName.Length) ? Color.red : Color.white);
			num++;
		}
	}

	private int GetNextContentsCursor(bool isBack = false, bool x_skip = false)
	{
		int contentsIndex = m_contentsIndex;
		int num = ((!x_skip) ? 1 : CONTENTS_FONT_X_COUNT);
		if (isBack)
		{
			num = -num;
		}
		int count = m_fontList[m_currentCursor].Count;
		if (count <= 1)
		{
			return contentsIndex;
		}
		contentsIndex += num;
		if (contentsIndex >= count)
		{
			contentsIndex = (x_skip ? (contentsIndex % CONTENTS_FONT_X_COUNT) : 0);
		}
		else if (contentsIndex < 0)
		{
			if (x_skip)
			{
				int num2 = count / CONTENTS_FONT_X_COUNT + 1;
				int num3 = num2 * CONTENTS_FONT_X_COUNT + contentsIndex;
				contentsIndex = ((num3 < count) ? num3 : ((num2 - 1) * CONTENTS_FONT_X_COUNT + contentsIndex));
			}
			else
			{
				contentsIndex = (contentsIndex + count) % count;
			}
		}
		return contentsIndex;
	}

	private int GetSkipNum(bool isBack)
	{
		switch (m_MenuElements[m_currentCursor].MenuText.text)
		{
		case "め":
			if (!isBack)
			{
				return 3;
			}
			return 5;
		case "も":
			if (!isBack)
			{
				return 3;
			}
			return 5;
		case "れ":
			if (!isBack)
			{
				return 5;
			}
			return 4;
		case "や":
		case "ゆ":
		case "よ":
			if (!isBack)
			{
				return 3;
			}
			return 5;
		default:
			return 5;
		}
	}

	protected override void SceneExit()
	{
		Scene sceneByName = SceneManager.GetSceneByName("NetworkOptions");
		if (sceneByName.IsValid())
		{
			sceneByName.GetRootGameObjects().First().GetComponent<NetworkOptions>()
				.SetShopName(m_shopName);
		}
	}
}
