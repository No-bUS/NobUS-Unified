task Clean {
  @("obj", "bin")
  | ForEach-Object { Get-ChildItem -Recurse -Filter $_ -Directory }
  | ForEach-Object -ThrottleLimit 9999 -Parallel {
    $emptyFolder = New-Item -Path $_.Parent -Name "$($_.Name)$(Get-Random).empty" -ItemType Directory
    & Robocopy.exe $emptyFolder $_ /MIR | Out-Null
    Remove-Item -Path $emptyFolder, $_ -Recurse -Force
  }
}

task Format {
  exec { csharpier format . }
}

task BuildAndroid {
  $targetName = Select-String -Path "Directory.Build.props" -Pattern "net[0-9]\.0" | Select-Object -ExpandProperty Line | Select-String -Pattern "net[0-9]\.0" -AllMatches | Select-Object -ExpandProperty Matches | Select-Object -ExpandProperty Value
  exec { dotnet publish "./src/NobUS.Frontend.MAUI/NobUS.Frontend.MAUI.csproj" -f "$($targetName)-android" -p:TargetFrameworkIdentifier=".NETCoreApp" -c release -r android-arm64 -p:AndroidSdkDirectory=$env:ANDROID_HOME -v diag }
}

task MoveApk {
  $apkPath = Resolve-Path "./src/NobUS.Frontend.MAUI/bin/release/net[0-9].0-android/*/publish/*-Signed.apk"
  Move-Item -Path $apkPath -Destination "./artifacts/NobUS.apk" -Force
}

task InstallApk {
  exec { adb install -r "./artifacts/NobUS.apk" }
}

task ProduceApk BuildAndroid, MoveApk
task DeployApk ProduceApk, InstallApk
