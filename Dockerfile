# Based it off of alpine
FROM alpine:latest AS build
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

# Base off of a fresh alpine image, so that we can only include the required
# runtime dependencies
FROM alpine:latest
# cd into /app
WORKDIR /app
# copy from the previous container
COPY --from=build /app .
# Install the ASP.NET runtime
RUN apk add aspnetcore7-runtime
# Set the default port to 80
ENV ASPNETCORE_URLS=http://+:80
# When we run the Docker image, this will be run by default
CMD ["/app/blog"]
