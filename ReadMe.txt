Keystore debug pass: android 
Create SHA1 Fingerprint: keytool -list -v -keystore C:\Users\Admin\.android\debug.keystore -alias androiddebugkey -storepass android -keypass android

Create keyhashes:
keytool -exportcert -alias keystorealias -keystore C:\yourkeystore\folder\keystore.jks | openssl sha1 -binary | openssl base64

#In this project
1.For debug:
keytool -exportcert -alias androiddebugkey -keystore C:\Users\NC\.android\debug.keystore | openssl sha1 -binary | openssl base64
2.For release:
keytool -exportcert -alias androiddebugkey -keystore C:\Users\NC\.android\debug.keystore | C:\openssl\bin\openssl sha1 -binary | C:\openssl\bin\openssl base64
2.Migrate keystore:
keytool -importkeystore -srckeystore E:\UnityProject2017\FACEBOOK\AppConsole\applog.keystore -destkeystore E:\UnityProject2017\FACEBOOK\AppConsole\applog.keystore -deststoretype pkcs12

#Open logcat
Android/sdk/platform-tools run: adb logcat

#Plugins in this project
FirebaseSDK
facebook-unity-sdk-7.10.1.unitypackage
play-services-resolver-1.2.59.0.unitypackage


