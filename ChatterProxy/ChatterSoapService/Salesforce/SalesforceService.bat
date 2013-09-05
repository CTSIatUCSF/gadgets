@Set Path=C:\WINDOWS\Microsoft.NET\Framework64\v3.5;

"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\wsdl.exe" SalesforceService.wsdl /language:CS /n:Salesforce /urlkey:SalesForceUrl /protocol:SOAP /out:SalesforceService.cs
"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\wsdl.exe" SalesforceApex.wsdl /language:CS /n:Salesforce.apex /urlkey:SalesForceUrl /protocol:SOAP /out:SalesforceApex.cs

