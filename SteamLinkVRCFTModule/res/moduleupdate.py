import json
import datetime
from json import JSONEncoder
f = open("module.json")

data = json.load(f)
versionstr = data["Version"]
x,y,z = versionstr.split(".")
z = str(int(z)+1)
data["Version"] = x+ "." + y + "." + z
data["LastUpdated"] = datetime.datetime.now().isoformat()
f.close()
f = open("module.json", "w+")
json.dump(data, f, ensure_ascii=False, indent=4)
f.close()
