FROM mcr.microsoft.com/dotnet/sdk:3.1-focal AS build-env
COPY ./iac/docker/* ./
# COPY ./install-extensions.bash ./
ENV AZURE_CLI_VERSION=2.25.0-1
RUN ./install-azure-cli.bash
RUN ./install-azure-core-tool.bash
RUN ./install-psql.bash
RUN az config set extension.use_dynamic_install=yes_without_prompt
RUN apt-get update
RUN apt-get install nano
RUN mkdir -p /home/piipan && adduser --disabled-password piipan
WORKDIR /home/piipan
COPY . .
# RUN chown -R piipan:piipan /home/piipan
# RUN chmod -R ug+rwx /home/piipan
# USER piipan
RUN mkdir app
RUN ./iac/install-extensions.bash
RUN az cloud set --name AzureCloud
# CMD ["az","login"]

