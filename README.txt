
	============================================
	  Export des résultats d'analyse Highlight
	============================================


Prérequis
---------

Le runtime .NET Core 2.1 doit être installé sur la machine et disponible dans le PATH de l'utilisateur.


Utilisation - KPIs
------------------

Le fichier Run_Highlight_Export.bat permet de faciliter le lancement de l'outil.

Il contient en particulier 2 paramètres qui doivent être modifiés afin de correspondre à votre organisation :
	- EMAIL_ADDRESS : doit contenir l'adresse email du compte Highlight utilisé pour se connecter
	- DOMAINID : doit contenir l'identifiant du domaine cible (récupérable via l'URL de la page "Gérer les applications")

Le fichier batch attend 3 arguments :
	- 1er  argument = mot de passe du compte indiqué dans le fichier batch
	- 2ème argument = nom du template
	- 3ème argument = nom du fichier de sortie

Exemple :

	C:\> Run_Highlight_Export.bat MotDePasse template.txt resultat.txt

Si des paramètres additionnels sont présents, ils seront passés à l'outil. Par exemple, pour ne récupérer que les résultats de l'application correspondant à l'ID 54321 :

	C:\> Run_Highlight_Export.bat MotDePasse template.txt resultat.txt --appid 54321


Utilisation - Logs
------------------

Le fichier Run_Highlight_Audit.bat permet de faciliter le lancement de l'outil.

Il contient en particulier 2 paramètres qui doivent être modifiés afin de correspondre à votre organisation :
	- EMAIL_ADDRESS : doit contenir l'adresse email du compte Highlight utilisé pour se connecter
	- COMPANYID : doit contenir l'identifiant de la société ciblée
	- AUDIT_FILENAME : le nom du fichier Excel qui recevra les données d'audit

Le fichier batch attend 1 argument :
	- 1er  argument = mot de passe du compte indiqué dans le fichier batch

Exemple :

	C:\> Run_Highlight_Audit.bat MotDePasse


Aide en ligne
-------------

Pour obtenir de l'aide :

	C:\> dotnet HighlightKPIExport.dll -h
	HighlightKPIExport - Export Application KPIs from CAST Highlight

	Usage:

		HighlightKPIExport.exe [options]

	Available options:

		   *** CONNECTION INFORMATION ***
		--url [value], -s [value]: base URL of CAST Highlight server; default is https://rpa.casthighlight.com
		--user [value], -u [value]: user id; typically an email address
		--password [value], -p [value]: password; if this is the name of an existing file, the password is read from this file
		--credentials [value], -c [value]: credential file name; the file must contain used id and password separated by a colon, e.g. "me@acme.com:my_Pa$$W0rd"

		   *** KPI EXPORT ***
		--domainid [value], -d [value]: id of an Highlight domain; mandatory; multiple occurrences are allowed
		--appid [value], -a [value]: id of an Highlight application; optional; multiple occurrences are allowed; if no app id is provided, KPIs for all applications from the specified domain(s) will be extracted
		--template [value], -t [value]: file name of the template file; by default, "template_scorecard.html" for a single application, "template_csv.txt" for multiple applications
		--output [value], -o [value]: file name of the result file; by default, results are displayed on the standard output

		   *** AUDIT LOG EXPORT ***
		--companyid [value], -l [value]: id of an Highlight company; optional; multiple occurrences are allowed
		--auditfile [value], -af [value]: file name of the audit file; by default "highlight_audit_{companyid}_{timestamp}.xlsx"

		   *** MISC. ***
		--verbose, -v: turn verbosity on
		--help, -h: display help information
