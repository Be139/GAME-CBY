import json,sys
s=open(sys.argv[1],encoding='utf-8',errors='replace').read()
i=s.find('{')
try:
    j=json.loads(s[i:])
    def walk(o):
        if isinstance(o,dict):
            for k,v in o.items():
                if isinstance(v,str) and '\n' in v and len(v)>40: print(v); return True
                if walk(v): return True
        return False
    if not walk(j): print(s[:4000])
except Exception as e:
    print('ERR',e); print(s[:4000])
