@Set Path=C:\WINDOWS\Microsoft.NET\Framework\v3.5;

"C:\Program Files\Microsoft SDKs\Windows\v7.0A\bin\wsdl.exe" SalesforceService.wsdl /language:CS /n:Salesforce /urlkey:SalesForceUrl /protocol:SOAP /out:SalesforceService.cs
"C:\Program Files\Microsoft SDKs\Windows\v7.0A\bin\wsdl.exe" SalesforceApex.wsdl /language:CS /n:Salesforce.apex /urlkey:SalesForceUrl /protocol:SOAP /out:SalesforceApex.cs

