#!/bin/bash
#
# Wrapper for the func azure functionapp publish.
# The function help with the robusness of the IaC code. 
# In ocassions the original fuction can fail, because the resource is not 
# availble yet, or other kind of error. The wrapper function will try 
# to publish the functionapp up to an MAX_TRIES of times. 
#
# app_name - name of the function application
# directory - path to the application project
#
# usage:   source ./azfunc-publish.bash
#          azfunc_publish <app_name> <directory>

azfunc_publish () {
  app_name=$1
  directory=$2

  echo "Waiting to publish function app"
  sleep 30

  ERR=0 # or some non zero error number you want
  MAX_TRIES=6

  echo "Publishing ${app_name} function app"
  pushd "$directory"
    for (( i=1; i<=MAX_TRIES; i++ ))
      do
        ERR=0
        echo "Waiting to publish function app"
        sleep $(( i * 30 ))

        echo "func azure functionapp publish ${app_name} --dotnet" 
        func azure functionapp publish "$app_name" --dotnet || ERR=1

        if [ $ERR -eq 0 ];then
          (( i = MAX_TRIES + 1))
        fi

      done
    if [ $ERR -eq 1 ];then
      echo "Too many non-sucessful tries"
      exit $ERR
    fi
  popd

}