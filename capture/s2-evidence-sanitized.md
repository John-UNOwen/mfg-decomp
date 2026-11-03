# S2 provider capture - sanitized evidence (generated from output_log.txt 2026-10-24)

## eAC3 service bootstrap (XRPC verbs logged by the client)
- cardmng.authpass  x2
- cardmng.bindmodel  x1
- cardmng.inquire  x4
- vfgac.send_paylog  x2
- vfgac.service_list  x3
- vfgac.update_refer  x3
- vfglog.put_msg  x1

## service_list answer (provider front topology)
- [S2CAP] service_list service_url=http://<provider-host>/aog/ services=2
- [S2CAP] service name=front url=http://<provider-host>/aog/ mode=operation
- [S2CAP] service name=game url=http://<provider-host>/aog/ mode=operation
- [S2CAP] FrontServerUrl set to: http://<provider-host>/aog/
- [S2CAP] service_list service_url=http://<provider-host>/aog/ services=2

## Front APIs used, whole session (counts)
- AppliInfo x5
- ChkTabooword x1
- ClientStateRead x4
- ClientStateWrite x40
- CreatePlayer x1
- DojoGetStatus x3
- EndGame x2
- EntryGame x2
- GachaInfoRequest x2
- GChat x39
- GetMenuData x2
- GetRecord x4
- GGet x326
- GPost x166
- Login x2
- Logout x1
- MissionDate x2
- PresentDone x2

## Sample request shapes (masked)
- [2026-10-24 01:18:28] 28:524 Login.PushRequest--> url: http://<provider-host>/aog//login data: system_id=<PCBID>&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&user_id=<user-id>&bypass_auth=true&guest=0
- [2026-10-24 01:33:54] 54:151 GGet.PushRequest--> url: http://<provider-host>/aog//gget data: system_id=<PCBID>&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&pcuid=<PCUID>&gserv_id=1&ready=1&must=2.0.2/<PCUID>/1027/0/1466323762/554
- [2026-10-24 01:33:54] 54:741 GPost.PushRequest--> url: http://<provider-host>/aog//gpost data: system_id=<PCBID>&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&pcuid=<PCUID>&gserv_id=1&tid=1027&mid=1466323762&pindex=0&must=2.0.2/<PCUID>/1027/0/1466323762/555/2/13/0/17/0/0/0/1/0&app_param=0/0
- [2026-10-24 01:34:07] 7:431 EndGame.PushRequest--> url: http://<provider-host>/aog//end_game data: system_id=<PCBID>&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&pcuid=<PCUID>&tid=1027&pindex=0&mid=1466323762&version=2.0.2

## Match command vocabulary client->server (gpost)
- NAKINASHI x26
- NEXT_KYOKU_READY x8
- SUTE_PAI x132

## Manual input proof (selected tile discarded rather than drawn tile)
- GPost param: SUTE_PAI(2). pindex:0 next_sno:373 tepai_pos:11 tepai_pos2:0 pai_id:32(32) tepai_id:0(0) tepai_id2:0(0) reach_flg:0 tsumogiri_flg:0 chi_naki:0

## Settlement + result + billing
- [2026-10-24 01:15:36] 36:415 EndGame.PushRequest--> url: http://<provider-host>/aog//end_game data: system_id=<PCBID>&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&pcuid=<PCUID>&tid=1026&pindex=0&mid=1466323762&version=2.0.2
- [2026-10-24 01:15:36] 36:760 EndGame--> resultcode:0 sec:0.3
- PlayerResultInfo Parse: index=0, rank=3, score=17300, uma=-20000
- PlayerResultInfo Parse: index=1, rank=1, score=25200, uma=10000
- PlayerResultInfo Parse: index=2, rank=2, score=24200, uma=-10000
- PlayerResultInfo Parse: index=3, rank=0, score=33300, uma=20000
- send_paylog xml : eaSTART_PASELI100
- [2026-10-24 01:34:07] 7:431 EndGame.PushRequest--> url: http://<provider-host>/aog//end_game data: system_id=<PCBID>&sic=VFG:J:A:A:2026060900&loc_id=ea&web_id=1&area_id=1&pcuid=<PCUID>&tid=1027&pindex=0&mid=1466323762&version=2.0.2
