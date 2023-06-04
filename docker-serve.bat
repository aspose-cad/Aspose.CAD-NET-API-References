docker build -f Dockerfile -t aspose-api-docs-net .
docker run -it -p 8080:8080 --name aspose-api-docs-net aspose-api-docs-net