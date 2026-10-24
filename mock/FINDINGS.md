# FINDINGS

Mock response templates for the front-server surface of the decompiled MFG build
(`api/recovered/Assembly-CSharp/MFG.GameRequest/`), derived from each endpoint class `OnParse`.

## Rules that shaped every template

- `RequestApiBase.ParseResultStatusCode` (`MFG.GameRequest/RequestApiBase.cs:160-179`) reads
  `root.NeedNotElement("serv_st")` first. Without `<serv_st>` the endpoint `OnParse` is aborted
  before the payload is read (swallowed at `RequestApiBase.cs:116`); with `<serv_st>` the `code`
  is read from `serv_st`, never from the root. So every spec carries
  `<serv_st><code>0</code></serv_st>`. The root level `<code>0</code>` is decorative parity with
  the stated minimal body; the parser never reads it.
- A body with no `code` node lands on the `num = 630` default and throws
  `NetworkResponseException`, so `code` must be present and zero. "missing code means OK" is not
  true in this assembly.
- Payload nodes are read off the root (`baseElem`), so they are siblings of `serv_st`.
- `MustNeedElement` / `NeedNotElement` throw when the child element is missing. `MustNeed` is
  swallowed only where the endpoint wraps it; `NeedNot` inside an endpoint parse aborts the rest
  of that payload. Nodes reached through unguarded chained `.Element(...)` /
  `AttributeValueInt` / `ValueInt` are hard required too (NullReferenceException is not caught).
- `MFG.Sytem.Utilities.Xml/Extensions.cs` value helpers are strict parses: `ValueInt`,
  `ValueUInt`, `ValueInt64`, `ValueUInt64`, `ValueBool`, `ValueArrayInt` (space separated),
  `ValueToDateTime`, `ValueUInt64ToDateTime` (unix seconds),
  `AttributeValueJavaCurrentTimeMillsToDateTime` (epoch millis). Attribute forms are emitted as
  attributes, not child elements.
- Base64 nodes (`Convert.FromBase64String` then `SomeClass.LoadFromJson`, a `JsonUtility`
  deserialization) carry the smallest JSON that the typed class tolerates. Fields that the typed
  class does not have to see are left out; whole optional nodes were skipped rather than risk bad
  JSON.
- All 47 endpoint classes are POST. `RequestParameter.MethodType` defaults to POST and
  `HttpWebRequestor` only issues GET when the body is null (`HttpWebRequestor.cs:174-190`);
  every class sets `m_postStr`, and `Reconnect.cs` sends an empty body (Content-Length 0, POST).

## Endpoint table

| endpoint | parser class | required nodes | optional but modeled | skipped as optional |
|---|---|---|---|---|
| appli_boot | AppliBoot.cs | serv_st>code | server_setting>{mask_ac_link_scene,reviewed_version}; boot_mes>{status,moserv_url,message} (message must be numeric: read as text then ValueInt) | other server_setting/boot_mes children |
| appli_info | AppliInfo.cs | serv_st>code; expire_seconds (ValueUInt) | info_data[@kind=pro_stats] base64 ProStats; info_data[@kind=events] base64 GameEventStatus | other info_data kinds |
| login | Login.cs | serv_st>code | auth>{session_id (becomes pcuid), user_id echoed from the POST body} | any other auth field |
| create_player | CreatePlayer.cs | serv_st>code | none | none |
| get_menudata | GetMenuData.cs + PlayerGameData.cs:392 + GameModeInfo.cs:31 | serv_st>code; menudata | mpdata>{mid,name}; playmode_list>playmode>{gmode,taku_class,payment_mode,table_type,pmax,tenbo,state,rate,superior_border} (all 9 unguarded) | battle_item_settings, recon>gmode |
| client_state_read | ClientStateRead.cs | serv_st>code | state[@kind=version_game]>data base64 VersionGameData; state[@kind=player_game]>data base64 PlayerGameData | receive_only, player_config, player_item, customize_item, story, notice_game, present_box_game, mission, gacha, cc_request, competition_game, competition_latest_result, ea_shop, log, chara_stamp_event, event_taku, vote_event, player_paid_item, scramble_festival, friend_game, music_gacha, bingo_event_remake, others_event, shop_competition_game, shop_competition_latest_result |
| client_state_write | ClientStateWrite.cs | serv_st>code | none | none |
| player_record | GetRecord.cs + PlayerRecordGameData.cs + PlayerRecordInfo.cs:27 | serv_st>code | player_record>{total, info>{kind,rank_list>rank_count,yakuman_num,histories>history>{rank,score,is_yakuman,is_buttobi,date}}} | g2_scramble blocks, extra info/history rows |
| entry_game | EntryGame.cs + EntryTakuInfo.cs:47 | serv_st>code; entry>{gserv_id,tid,pindex,next_sno,last_cyoukou_num,cyoukou_num,ste_oya1_limit_time,ste_limit_time,ste_reechi1_limit_time,naki_limit_time,agari_limit_time,naki_choice_limit_time,reechi_choice_limit_time,last_cyoukou_limit_time,last_time,gserv_url,pay_mode} | none | preference_matching>entry_matching_official_player[@entry_id,@priority,@entry_unit]>unit_left |
| gget | GGet.cs + MatchingData.cs + TaikyokuData.cs:19 | serv_st>code | game>{all_ready, mwait>{status,pnum,cpu_num,pindex}, taikyoku>cell_info[@available]>cell_sno[@start={{seq}},@count]} | mend roster, epdata_N, pinfo_N, chat/d |
| gpost | GPost.cs (same parse helpers as gget) | serv_st>code | game>{mwait>{status,pnum,cpu_num,pindex}, taikyoku>cell_info>cell_sno[@start={{seq}}]} | recon (TaikyokuStartInfo), chat/d, epdata_N, pinfo_N |
| gchat | GChat.cs + StampData.cs:27 | serv_st>code | chat>d[@idx={{seq}},@mid,@pindex,@time]>{name,contents,param} | extra d entries |
| gget_stamp_info | GGetStampInfo.cs + StampData.cs | serv_st>code | stamp_info>d[@idx,@mid,@pindex,@time]>{name,contents,param} | extra d entries |
| end_game | EndGame.cs + MahjongResultInfo.cs:75 | serv_st>code; mgresult + {gmode,taku_class,continue_state,continue_fee} | breakdown>exp>{base_exp,group_adjust_exp}; breakdown>affinity_point>{affinity_growth_point,actual_affinity_growth_point}; player_0>{rank,score,uma} | mgresult>old, mgresult>new, battle_item_use_info, bonus_list entries, player_1..3, g2result |
| kiken_game | KikenGame.cs + MahjongResultInfo.cs:75 | serv_st>code; mgresult + {gmode,taku_class,continue_state,continue_fee} | breakdown exp/affinity_point with bonus_list entries; player_0>{rank,score,uma}; g2result>r>{id,sub_cate,win} | mgresult>old, mgresult>new, battle_item_use_info, player_1..3 |
| keep_alive | KeepAlive.cs | serv_st>code | none | none |
| gacha_info | GachaInfoRequest.cs + GachaInfoGameData.cs:54 | serv_st>code; gacha_schedule>info>{id,label,ticket_nr,now_active,force_active,series_type,items>item,pickup_charas>chara} | pickup_items/item, limited_pickup_items/item, exchange_items/item, pickup_rate>{normal,pickup,limited}, start_date, end_date | force_end_date, extra info entries |
| gacha_log | GachaLogSend.cs | serv_st>code | none | none |
| req_draw_gacha | ReqDrawGacha.cs | serv_st>code | transaction_info>transaction_id (fed back to get_gacha_result) | none |
| get_gacha_result | ReqGachaResult.cs | serv_st>code; lottery_result>data>{character_id,unique_id} | lottery_result, data | extra data entries, gift/point counters |
| music_gacha_play | MusicGachaPlayRequest.cs | serv_st>code; gacha_result>is_success; on success gain_items>item + gift | gain_items>item (voice object id) | fight_spirits |
| music_gacha_play_reserve | MusicGachaPlayReserve.cs | serv_st>code | gacha_reserve>{is_success,request_id (ValueUInt64, forwarded to music_gacha_play)} | none |
| cutin_gacha_play_apply | CutinGachaPlayApply.cs | serv_st>code | gacha_apply>{is_success,request_id} | none |
| cutin_gacha_play_draw | CutinGachaPlayDraw.cs | serv_st>code; gacha_draw>{is_success,request_id}; on success gain_items>item>{oid,gift_type} + gift | fight_spirits>item | extra gain_items entries |
| get_jongstone_info | GetJongStoneInfo.cs | serv_st>code; jongstone_info>{free_point,record_point} (no null check anywhere) | none | none |
| dojo_get_status | DojoGetStatus.cs | serv_st>code; dojo>slot_nr; dojo>slot[@idx]+slot/available; available=1 adds {character_obj,start_time,next_time,has_next,reserve_souls,all_souls} | 4 slots (idx 0 available, idx 1..3 empty) | none |
| dojo_gain_soul | DojoGainSoul.cs | serv_st>code; dojo>{slot_id,get_nr,slot} + slot body rules | post-harvest slot body | none |
| dojo_set_slot | DojoSetSlot.cs | serv_st>code; dojo>{slot_id,updated,slot} + slot body rules | updated=1, reserve_souls=0 | none |
| mission_date | MissionDate.cs + MissionStatus.cs + MissionData.cs | serv_st>code; per included info_data: @kind + base64 body | info_data[@kind=missions] base64 {list:[{category,active,begin,end,param}]} | other info_data kinds, other mission categories |
| competition_entry | CompetitionEntry.cs | serv_st>code; entry present implies entry>{result,is_entry}; displayed_result present implies displayed_result>{result,is_check_result} | entry (result 1, is_entry 1), displayed_result (result 1, is_check_result 1) | failure reason text |
| present_done | PresentDone.cs + PresentBoxGameData.cs:162 | serv_st>code; present>data>{id,success}; success nonzero adds {content,amount} | one data row | extra data rows per present id |
| notice_done | NoticeDone.cs | serv_st>code | none | none |
| important_notice_done | ImportantNoticeDone.cs | serv_st>code | none | none |
| odekake_done | OdekakeDone.cs | serv_st>code; odekake>{id,success}; success nonzero makes every state need @kind + state/data base64 | state kind=player_game base64 PlayerGameData, state kind=customize_item base64 CustomizeGameData | full CharaGameData/item blobs, success=0 variant |
| eashop_done | CoopDone.cs (class name differs from path) | serv_st>code | none | none |
| chk_tabooword | ChkTabooword.cs | serv_st>code | taboo_chk>result=0 (result is unguarded once taboo_chk exists; IsAvailable = !result) | none |
| chara_enabled | CharaEnabledSend.cs | serv_st>code | is_gacha_enabled=1 paired with end_date (JST text) | none |
| set_favorite_character | SetFavoriteCharacter.cs | serv_st>code | none | none |
| janpon_log | JanponLog.cs | serv_st>code | none | none |
| item_gain_log | ItemGainLog.cs | serv_st>code | none | none |
| item_consume_log | ItemConsumeLog.cs | serv_st>code | none | none |
| end_show | EndShow.cs + ShowResultInfo | serv_st>code; showresult | voltage, contribute_percent, card_effect_percent, item_effect_percent, bonus, get_point | showresult>old, showresult>new (no-op Element() calls) |
| logout | Logout.cs | serv_st>code | none | none |
| get_mg | GetMg.cs | serv_st>code; mg_info>{mg,additional_mg} (chained, no guard) | none | none (path not derivable, see uncertain list) |
| get_haifu_list | GetHaifuList.cs | serv_st>code; haifu_list>{any child}>{gmode,taku_class,tid,pindex,end_date,name0..3,rank0..3,score0..3} | copy_tid | none |
| get_haifu_data | GetHaifuData.cs + HaifuData (positional core_io) | serv_st>code; core_io(1)>in>tmake>play_mode>{gmode,pnum}; core_io(2)>out>tmake>{zaseki_0..3,assign_cpu>{cpu_0..2}} | in>tmake>pinfo_0, core_io(2)>out>taikyoku>cell_info>{cell_sno@count,cell_data_0@kind=11} | real cell_data payloads (kinds 1-24), extra core_io |
| reconnect | Reconnect.cs (path style URL, not DefaultFrontServerUrl) | serv_st>code; entry>{gserv_id,tid,pindex,next_sno,last_cyoukou_num,cyoukou_num,ste_oya1_limit_time,ste_limit_time,ste_reechi1_limit_time,naki_limit_time,agari_limit_time,naki_choice_limit_time,reechi_choice_limit_time,last_cyoukou_limit_time,last_time,gserv_url,pay_mode} | none | none |

## Most uncertain templates

- get_mg.xml: `OnCreateSendParameter` is empty in the decompile, so the real path is inferred from
  the `get_haifu_*` naming convention; the mock routes it by substring, and the decompile also
  assigns both `mg` and `additional_mg` to the same field.
- reconnect.xml: `entry>gserv_url` has to be a path fragment that survives `Split('/')[3]`
  int-parsing in `TaikyokuGameData.Apply`; the value used is a guess.
- gget.xml / gpost.xml: `mwait` status polarity and the cell window shape (available/cell_sno/
  cell_data kinds) are guesses; a real match needs `cell_data_<n>` payloads for gameplay commands.
- kiken_game.xml / end_game.xml: `bonus_type_id` and `sub_cate` vocabularies, and whether `old` /
  `new` PlayerGameData snapshots are required for the result flow to render.
- get_menudata.xml: playmode `gmode`/`taku_class`/`payment_mode`/`table_type` values are
  placeholders; a real server lists several modes with real rate and border values.
- gacha_info.xml: `info>id` must resolve to a `GachaSeriesName` master entry (2 = PickupPine) and
  item oids are in `CutinItemMaster.ObjectID` form; both are guesses about master data.
- chara_enabled.xml / mission_date.xml: date strings are parsed with `DateTime.TryParse` under the
  current culture and assumed to be JST.
