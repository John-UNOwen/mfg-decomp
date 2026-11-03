import json
d=json.load(open(r'C:\mfg\build\export-primary-content\Assets\Shaders\sg_cha_outline.json',encoding='utf-8-sig'))
pf=d['m_ParsedForm']
props=pf['m_PropInfo']['m_Props']
print('PROP COUNT', len(props))
for i,p in enumerate(props):
    print(' prop',i,p['m_Name'],'type',p['m_Type'],'def0',p['m_DefValue_0_'],'def1',p['m_DefValue_1_'],'def2',p['m_DefValue_2_'],' desc',p['m_Description'],'flags',p['m_Flags'])
