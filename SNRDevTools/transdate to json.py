import time
writedate='''{
'''
readdate=""
with open("transdate.txt",mode="r",encoding="utf-8") as f:
    readdate=f.read().split("\n")
#writedate=writedate+'\n "AllLang" : "'+readdate[0].replace("moto,","")+'" ,\n'
for date in readdate[1:]:
    writedate=writedate+'\n "'+date.split(",")[0]+'" : {\n'
    for date2 in range(len(date.split(",")[1:])):
        writedate=writedate+"        \""+readdate[0].split(",")[(date2+1)]+'" : "'+date.split(",")[(date2+1)].encode('unicode-escape').decode('utf-8')+'" ,\n'
    writedate=writedate[:-3]+"\n        } ,"
writedate=writedate[:-1]+"\n\n}"
with open("translatedate.json",mode="w",encoding="utf-8") as f:
    f.write(writedate)
with open(r"..\SuperNewRoles\Resources\translatedate.json",mode="w",encoding="utf-8") as f:
    f.write(writedate)
print("完了")
time.sleep(2)
#input("Enter Key Click End.(Enterキーを押すと終了します。)")