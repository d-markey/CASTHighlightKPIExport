@echo off

REM for help try :
REM dotnet HighlightKPIExport.dll -h

REM identifiant Highlight du compte utilisé pour la connexion (idéalement l'adresse email d'un compte de service)
set EMAIL_ADDRESS=d.markey+SAUR@castsoftware.com

REM l'identifiant du domaine souhaité
REM cet identifiant peut être récupéré en ouvrant la page "Gérer les applications"
REM l'URL de cette page est de la forme https://rpa.casthighlight.com/#BusinessUnits/1234/Applications où 1234 est l'identifiant du domaine sélectionné
set DOMAINID=6491

REM arguments de la ligne de commande
REM 1ère position = mot de passe ou nom du fichier contenant le mot de passe
set PWD=%1
shift
REM 2ème position = nom du template
set TEMPLATE=%1
shift
REM 3ème position = nom du fichier de sortie
set OUTPUT=%1
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
dotnet HighlightKPIExport.dll --user %EMAIL_ADDRESS% --password %PWD% --domainid %DOMAINID% --template %TEMPLATE% --output %OUTPUT% --verbose %OTHER_ARGS%

REM lancement du fichier de sortie
start %OUTPUT%

echo on
