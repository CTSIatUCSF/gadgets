ALTER DATABASE profiles_102 SET TRUSTWORTHY ON

exec sp_configure 'clr enable', 1
GO
RECONFIGURE
GO

CREATE ASSEMBLY [SMdiagnostics]
AUTHORIZATION dbo
FROM 'C:\WINDOWS\Microsoft.NET\Framework64\v3.0\Windows Communication Foundation\SMdiagnostics.dll'
WITH PERMISSION_SET = UNSAFE	
GO

CREATE ASSEMBLY [System.Runtime.Serialization]
AUTHORIZATION dbo
FROM 'C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\System.Runtime.Serialization.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY SystemWeb 
FROM 'C:\WINDOWS\MICROSOFT.NET\FRAMEWORK64\V2.0.50727\SYSTEM.WEB.DLL' 
WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY ChatterSoapService
AUTHORIZATION dbo
FROM 'C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\ChatterSoapService.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY [ChatterSoapService.XmlSerializers]
AUTHORIZATION dbo
FROM 'C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\ChatterSoapService.XmlSerializers.dll'
WITH PERMISSION_SET = UNSAFE
GO


CREATE PROCEDURE [UCSF].[CreateChatterActivity]
@url nvarchar(500),
@userName nvarchar(50),
@password nvarchar(50),
@token nvarchar(100),
@createdDT datetime,
@externalMessage bit,
@employeeId nvarchar(50),
@actUrl nvarchar(255),
@actTitle nvarchar(255),
@actBody nvarchar(255)
AS EXTERNAL NAME ChatterSoapService.[ChatterService.ChatterSqlProcedures].CreateActivity
GO


drop PROCEDURE [UCSF].CreateChatterActivity
drop ASSEMBLY [ChatterSoapService.XmlSerializers]
drop ASSEMBLY ChatterSoapService
