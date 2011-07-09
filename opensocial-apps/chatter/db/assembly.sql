ALTER DATABASE Profiles SET TRUSTWORTHY ON

sp_configure 'clr enable', 1
GO
RECONFIGURE
GO

CREATE ASSEMBLY [SMdiagnostics]
AUTHORIZATION dbo
FROM 'C:\WINDOWS\Microsoft.NET\Framework\v3.0\Windows Communication Foundation\SMdiagnostics.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY [System.Runtime.Serialization]
AUTHORIZATION dbo
FROM 'C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\System.Runtime.Serialization.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY ChatterService
AUTHORIZATION dbo
FROM 'C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\ChatterService.dll'
WITH PERMISSION_SET = UNSAFE
GO

CREATE ASSEMBLY [ChatterService.XmlSerializers]
AUTHORIZATION dbo
FROM 'C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.5\ChatterService.XmlSerializers.dll'
WITH PERMISSION_SET = UNSAFE
GO


CREATE PROCEDURE usp_CreateChatterActivity
@url nvarchar(500),
@userName nvarchar(50),
@password nvarchar(50),
@token nvarchar(100),
@userId nvarchar(50),
@message xml
AS EXTERNAL NAME ChatterService.[ChatterService.ChatterSqlProcedures].CreateActivity
GO


--drop PROCEDURE usp_CreateChatterActivity
--drop ASSEMBLY [ChatterService.XmlSerializers]
--drop ASSEMBLY ChatterService
