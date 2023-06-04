set token=eyJhbGciOiJSUzI1NiIsImtpZCI6IjgyODc2NEU3OTA1MDA3QUQxOEY5OTc5RDZGNkEzMkJDQjgzQTJEQ0QiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJnb2RrNTVCUUI2MFktWmVkYjJveXZMZzZMYzAifQ.eyJuYmYiOjE2NzYxMTIyNDksImV4cCI6MTcwNzY0ODI0OSwiaXNzIjoiaHR0cHM6Ly9pZC5zdmMuY29uaG9sZGF0ZS5jbG91ZCIsIlBTUC5Vc2VySWQiOiI1ODM2OTYiLCJDb250YWluZXJpemUuVHlwZSI6IlBlcm1hbmVudCB0b2tlbiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiZGVuaXMuZGVtZW5rb0Bhc3Bvc2UuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IkRlbWVua28iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9naXZlbm5hbWUiOiJEZW5pcyIsImF1ZCI6ImF1ZCJ9.TIT80T-dUAxs-76tm6KOmi5Cr5pXMsq9uXBZwOCq1uVtevU92lUe8obWrA1S71l3VxhcqFpHShw0KDaQq_la1CZac14YBvTbZBGnYxYhOU4gjaasDWA911m1PAhttri6qX90HPkWgXEUH417xShdrKjl-VKSq2GEFMB0g0jErgMEORThj8Y6LyHDfLRoFWruaNQkcgMq3KxtwBnrwAISSaiFCBOdjgWo9X69cz-5PgxfKnxUqfixjUt1fxowdTGFf9ljhidwIOPlMYVS6dlhwKLrm9rEydQsb8HIdth8Xa-4Y6GuVuOcoZUHImmK912fqsIoHBRThAx2czKmhoUC9Q 

set conhodateId=r2nl0flc

set user="denis.demenko"
set pass="]x)AQ8DDk[Uy|8SRR?:C"

set api=aspose-api-docs-net
set docker_registry=cad.repository.dev.dynabic.com

docker build -f Dockerfile -t %api% .
IF %ERRORLEVEL% == 0 (
    (git log -n 1 --pretty=format:%%h) > tmpFile.txt
    git log -n 1 --pretty=format:%%h
    set /p tag= < tmpFile.txt
    REM del tmpFile.txt
    echo tag: %tag%

    docker tag %api%:latest %docker_registry%/%api%:%tag%

    devcon context login --replace -t %token%
    echo | set /p=%password% | docker login -u %user% --password-stdin %docker_registry%
    
    docker push %docker_registry%/%api%:%tag%
	IF %ERRORLEVEL% == 0 (
		echo successfully pushed new image
		GOTO Deploy
	)
	echo failed to push
)
echo failed to build
GOTO End

:Deploy

devcon deploy --id %conhodateId% --file cloudfile.yaml --force --image %docker_registry%/%api%:%tag%
echo https://%api%-%conhodateId%.k8s.dynabic.com
echo successfully deployed

:End