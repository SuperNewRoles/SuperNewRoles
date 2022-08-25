import glob
files=glob.glob("trans_**_**.txt")
readlist={}
with open("transmoto.txt",mode="r") as f:
    motodate=f.read()
for file in files:
    with open(file,mode="r",encoding="utf-8") as f:
        readlist[str(file)[6:8]] = f.read()
splitlist={}
motosplit=motodate.split("\n")
for date in readlist:
    if readlist[date]=="":
        splitlist[date]=[]
        for a in range(len(motosplit)):
         splitlist[date].append("")
    else:
     splitlist[date]=readlist[date].split("\n")
wdate="moto,"
for splitdate in splitlist:
    if splitdate[0]=="0":
        wdate=wdate+splitdate[1]+","
    else:
        wdate=wdate+splitdate+","
wdate=wdate[:-1]
for ndate in range(len(motosplit)):
 wdate=wdate+"\n"+motosplit[ndate]
 for splitdate in splitlist:
     print(splitlist[splitdate][ndate])
     wdate=wdate+","+splitlist[splitdate][ndate]
with open("transdate.txt",mode="w",encoding="utf-8") as f:
 f.write(wdate)
print("完了")
#input("Enter Key Click End.(Enterキーを押すと終了します。)")
