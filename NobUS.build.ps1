task Clean {
  @("obj","bin")
  | ForEach-Object { Get-ChildItem -Recurse -Filter $_ -Directory }
  | ForEach-Object -ThrottleLimit 9999 -Parallel {
    $emptyFolder = New-Item -Path $_.Parent -Name "$($_.Name)$(Get-Random).empty" -ItemType Directory
    exec { Robocopy.exe $emptyFolder $_ /MIR | Out-Null }
    Remove-Item -Path $emptyFolder,$_ -Recurse -Force
  }
}

task Format {
  exec { dotnet csharpier . }
  exec { prettier '**\*.{csproj,xml,xaml}' -w }
  exec { prettier '**\*.{csproj,xml,xaml}' -w }
}

task BuildAndroid {
  Set-Location -Path "./src/NobUS.Frontend.MAUI/"
  $targetName = Select-String -Path "NobUS.Frontend.MAUI.csproj" -Pattern "net[0-9]\.0-android" | Select-Object -ExpandProperty Line | Select-String -Pattern "net[0-9]\.0-android" -AllMatches | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value
  exec { dotnet publish -f $targetName -c release -r android-arm64 -p:AndroidSdkDirectory=$env:ANDROID_HOME }
}

task MoveApk {
  $apkPath = Resolve-Path "./src/NobUS.Frontend.MAUI/bin/release/net[0-9].0-android/*/publish/*-Signed.apk"
  Move-Item -Path $apkPath -Destination "./artifacts/NobUS.apk"
}

task InstallApk {
  exec { adb install -r "./artifacts/NobUS.apk" }
}

task ProduceApk BuildAndroid,MoveApk
task DeployApk ProduceApk,InstallApk
