@echo off

REM for help try :
REM dotnet HighlightKPIExport.dll -h

REM identifiant Highlight du compte utilisé pour la connexion (idéalement l'adresse email d'un compte de service)
set EMAIL_ADDRESS=user@acme.com

REM l'identifiant de la société visée pour l'audit
set COMPANYID=1234

REM le nom du fichier Excel
set AUDIT_FILENAME="HL_Audit_COMPANY_NAME_{timestamp}.xlsx"

REM arguments de la ligne de commande
REM 1ère position = mot de passe ou nom du fichier contenant le mot de passe
set PWD=%1
shift

REM récupération du reste de la ligne de commande
set OTHER_ARGS=
:readargs
if "%1"=="" goto run
set OTHER_ARGS=%OTHER_ARGS% %1
shift
goto readargs

:run
REM récupération des données depuis Highlight et génération du fichier de sortie
dotnet HighlightKPIExport.dll --user %EMAIL_ADDRESS% --password %PWD% --companyid %COMPANYID% --auditfile %AUDIT_FILENAME%

REM lancement du fichier de sortie
start %AUDIT_FILENAME%

echo on
