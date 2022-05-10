
# Export des résultats d'analyse [CAST Highlight](https://rpa.casthighlight.com)


## Prérequis

Ce logiciel nécessite un runtime .NET 5.0.


## Utilisation - KPIs

Exemple de ligne de commande:

```
HighlightKPIExport.exe --user user@acme.com --password XXX --domainid 54321 --template template_full.txt --output results.csv --verbose
```

Le fichier `Run_Highlight_Export.bat` permet de faciliter le lancement de l'outil.

Il contient en particulier 2 paramètres qui doivent être modifiés afin de correspondre à votre organisation :
* EMAIL_ADDRESS : doit contenir l'adresse email du compte [CAST Highlight](https://rpa.casthighlight.com) utilisé pour se connecter
* DOMAINID : doit contenir l'identifiant du domaine cible (récupérable via l'URL de la page "Gérer les applications")

Le fichier batch attend 3 arguments :
* 1er  argument = mot de passe du compte indiqué dans le fichier batch
* 2ème argument = nom du template
* 3ème argument = nom du fichier de sortie

Exemple :

```
Run_Highlight_Export.bat MotDePasse template.txt resultat.txt
```

Si des paramètres additionnels sont présents, ils seront passés à l'outil. Par exemple, pour ne récupérer que les résultats de l'application correspondant à l'ID 54321 :

```
Run_Highlight_Export.bat MotDePasse template.txt resultat.txt --appid 54321
```

## Utilisation - Logs

Le fichier `Run_Highlight_Audit.bat` permet de faciliter le lancement de l'outil.

Il contient en particulier 3 paramètres qui doivent être modifiés afin de correspondre à votre organisation :
* EMAIL_ADDRESS : doit contenir l'adresse email du compte [CAST Highlight](https://rpa.casthighlight.com) utilisé pour se connecter
* COMPANYID : doit contenir l'identifiant de la société ciblée
* AUDIT_FILENAME : le nom du fichier Excel qui recevra les données d'audit

Le fichier batch attend 1 argument :
* 1er  argument = mot de passe du compte indiqué dans le fichier batch

Exemple :

```
Run_Highlight_Audit.bat MotDePasse
```

## Aide en ligne

Pour obtenir de l'aide :

```
HighlightKPIExport.exe -h
```

Sortie:

```
HighlightKPIExport - Export Application KPIs and Audit Logs from CAST Highlight

Usage:

   dotnet HighlightKPIExport.dll [options]

Available options:
   
      *** CONNECTION INFORMATION ***
   --url [value], -s [value]: base URL of CAST Highlight server; default is https://rpa.casthighlight.com/
   --token [value], -tk [value]: token; if this is the name of an existing file, the token is read from this file; takes precedence over credential, user and password
   --user [value], -u [value]: user id (the email address the user registered with)
   --password [value], -p [value]: password; if this is the name of an existing file, the password is read from this file
   --credentials [value], -c [value]: credential file name; the file must contain user id and password separated by a colon, e.g. "me@acme.com:my_Pa$$W0rd"; takes precedence over user and password
   
      *** KPI EXPORT ***
   --domainid [value], -d [value]: id of an Highlight domain; mandatory; the company id is also a domain id; multiple occurrences are allowed
   --appid [value], -a [value]: id of an Highlight application; optional; multiple occurrences are allowed; if no app id is provided, KPIs for all applications from the specified domain(s) will be extracted
   --template [value], -t [value]: file name of the template file; by default, "template_scorecard.html" for a single application, "template_csv.txt" for multiple applications
   --output [value], -o [value]: file name of the result file; by default, results are displayed on the standard output
   
      *** AUDIT LOG EXPORT ***
   --companyid [value], -l [value]: id of an Highlight company; optional; multiple occurrences are allowed
   --auditfile [value], -af [value]: file name of the audit file; by default "highlight_audit_{companyid}_{timestamp}.xlsx"
   
      *** MISC. ***
   --maxconcurrency [value], -mc [value]: maximum number of concurrent requests; by default 3
   --verbose, -v: turn verbosity on
   --symbols, -sl: display list of available symbols
   --help, -h: display help information
```

## License

Le code source de ce logiciel est libre de droit et mis à la disposition de la communauté sous licence GNU GPL v3.

Ce logiciel fournit un exemple de code exploitant les [API de CAST Highlight](https://rpa.casthighlight.com/api-doc/index.html).

Il ne s'agit pas d'un outil officel fourni par la société CAST, et n'est pas officiellement maintenu.
