import mysql.connector
import os

print("running dbm script")

def CreateDB_only(Xcursor):
    Xcursor.execute("create database Face__Recog")

def CreatePeopleTableOnly(Xcursor):
    Xcursor.execute("create table people(EntryNumber  MEDIUMINT UNSIGNED NOT NULL AUTO_INCREMENT, Cam tinyint,ImageNumber MEDIUMINT UNSIGNED NOT NULL,Name varchar(40),Status varchar(20),ID varchar(30),InTime datetime,OutTime datetime,PRIMARY KEY (EntryNumber))")

def CreateEmployeesTableOnly(Xcursor):
    Xcursor.execute("create table employees(ID VARCHAR(30),Title VARCHAR(30),Full_Name VARCHAR(40),Date_Of_Birth DATE,Gender VARCHAR(10),Employment_Status VARCHAR(9),Mobile_1 VARCHAR(15),Mobile_2 varchar(15),Work_Email VARCHAR(50),Address VARCHAR(200))")

def CreateLogsTableOnly(Xcursor):
    Xcursor.execute("create table logs(EntryNumber  MEDIUMINT UNSIGNED NOT NULL AUTO_INCREMENT,ID VARCHAR(30),Full_Name VARCHAR(40),Cam tinyint,LogTime datetime,PRIMARY KEY (EntryNumber))")

dirname = os.path.dirname(__file__)

fh=open(os.path.join(dirname,'Config/host.txt'),'r')
HostName=str(fh.readline())
fh.close()

fh=open(os.path.join(dirname,'Config/user.txt'),'r')
UserName=str(fh.readline())
fh.close()

fh=open(os.path.join(dirname,'Config/password.txt'),'r')
Password=str(fh.readline())
fh.close()

HostName=HostName.replace("\n","")
UserName=UserName.replace("\n","")
Password=Password.replace("\n","")

MainDB= mysql.connector.connect(host=HostName,user=UserName,passwd=Password,auth_plugin='mysql_native_password')
MainCursor=MainDB.cursor(buffered=True)
DatabaseFound=False
MainCursor.execute("show databases")

for i in MainCursor:
    if("face__recog" in i):
        DatabaseFound=True
        break
if not(DatabaseFound):
    CreateDB_only(MainCursor)
    MainCursor.execute("use face__recog")
MainCursor.execute("use face__recog")

TableFound=True
try:
    MainCursor.execute("select * from people")
except:
    TableFound=False
if not (TableFound):
    CreatePeopleTableOnly(MainCursor)

EmpTableFound=True
try:
    MainCursor.execute("select * from employees")
except:
    EmpTableFound=False
if not (EmpTableFound):
    CreateEmployeesTableOnly(MainCursor)

LogsTableFound=True
try:
    MainCursor.execute("select * from logs")
except:
    LogsTableFound=False
if not (LogsTableFound):
    CreateLogsTableOnly(MainCursor)
