import time

writedata = '''{'''
readdata = ""

with open("TransData.txt", mode="r", encoding="utf-8") as f:
    readdata = f.read().split("\n")

for data in readdata[1:]:
    writedata = writedata+"\n    "+'\n"'+data.split(",")[0]+'": {\n'
    for data2 in range(len(data.split(",")[1:])):
        writedata = writedata+"        \""+readdata[0].split(",")[(data2+1)]+'" : "'+data.split(
            ",")[(data2+1)].encode('unicode-escape').decode('utf-8')+'" ,\n'
    writedata = writedata[:-3]+"\n    } ,"
writedata = writedata[:-1]+"\n\n}"

with open(r"..\SuperNewRoles\SuperNewRoles\Resources\TransData.json", mode="w", encoding="utf-8") as f:
    f.write(writedata)
print("完了")
time.sleep(2)
