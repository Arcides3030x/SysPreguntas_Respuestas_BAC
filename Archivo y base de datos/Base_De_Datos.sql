CREATE DATABASE PreguntasRespuestasDB;


USE PreguntasRespuestasDB;
GO

CREATE TABLE Usuarios (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Usuario NVARCHAR(50) NOT NULL UNIQUE,
    Contrasena NVARCHAR(100) NOT NULL,
    FechaRegistro DATETIME DEFAULT GETDATE(),
    Nombre NVARCHAR(50) NOT NULL,
    Apellido NVARCHAR(50) NOT NULL,
    Login NVARCHAR(50) NOT NULL,
    UrlFoto NVARCHAR(255),
    Estado TINYINT DEFAULT 1
);
GO

CREATE TABLE Preguntas (
    IdPregunta INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(255) NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL,
    IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(IdUsuario),
    EstaCerrada BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    Estado TINYINT DEFAULT 1
);
GO

CREATE TABLE FotosPreguntas (
    IdFotoPregunta INT IDENTITY(1,1) PRIMARY KEY,
    IdPregunta INT NOT NULL FOREIGN KEY REFERENCES Preguntas(IdPregunta),
    UrlFoto NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE Respuestas (
    IdRespuesta INT IDENTITY(1,1) PRIMARY KEY,
    Contenido NVARCHAR(MAX) NOT NULL,
    IdPregunta INT NOT NULL FOREIGN KEY REFERENCES Preguntas(IdPregunta),
    IdUsuario INT NOT NULL FOREIGN KEY REFERENCES Usuarios(IdUsuario),
    IdRespuestaPadre INT NULL FOREIGN KEY REFERENCES Respuestas(IdRespuesta),
    FechaCreacion DATETIME DEFAULT GETDATE(),
    Estado TINYINT DEFAULT 1
);
GO
