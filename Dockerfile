FROM mcr.microsoft.com/dotnet/sdk:3.1-focal AS build-env
COPY ./iac/install-azure-cli.bash ./install-azure-cli.bash
ENV AZURE_CLI_VERSION=2.25.0-1
RUN ./install-azure-cli.bash
RUN mkdir -p /home/piipan && adduser --disabled-password piipan
RUN chown -R piipan:piipan /home/piipan
RUN chmod -R ug+rwx /home/piipan
USER piipan
WORKDIR /home/piipan
COPY . .
CMD ["az","--version"]


