# Based it off of ubuntu
FROM alpine:latest

# Update the repo
RUN apk update
# Install dotnet sdk, so we can do `dotnet publish`
RUN apk add dotnet7-sdk
# Copy the project folder into the image
COPY ./ /src
# cd into the project folder
WORKDIR /src
# Compile the app into the /app directory
RUN dotnet publish -c Release -o /app
# Set the default port to 80
ENV ASPNETCORE_URLS=http://+:80
# When we run the Docker image, this will be run by default
CMD ["/app/blog"]
