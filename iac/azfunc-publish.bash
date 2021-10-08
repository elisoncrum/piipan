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