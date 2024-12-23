CREATE PROCEDURE CrearRespuesta
    @Contenido NVARCHAR(MAX),
    @IdPregunta INT,
    @IdUsuario INT,
    @IdRespuestaPadre INT = NULL,
    @Estado TINYINT,
    @IdRespuesta INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Respuestas (Contenido, IdPregunta, IdUsuario, IdRespuestaPadre, FechaCreacion, Estado)
    VALUES (@Contenido, @IdPregunta, @IdUsuario, @IdRespuestaPadre, GETDATE(), @Estado);

  
    SET @IdRespuesta = SCOPE_IDENTITY();
END;
GO


CREATE PROCEDURE ObtenerRespuestasPorPregunta
    @IdPregunta INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        r.IdRespuesta,
        r.Contenido,
        r.IdPregunta,
        r.IdUsuario,
        r.IdRespuestaPadre,
        r.FechaCreacion,
        r.Estado,
        p.Titulo AS PreguntaTitulo,            
        u.Usuario AS Usuario,                 
        u.Nombre AS UsuarioNombre,             
        rPadre.Contenido AS RespuestaPadreContenido, 
        rp.Titulo AS RespuestaPadreTitulo     
    FROM Respuestas r
    LEFT JOIN Preguntas p ON r.IdPregunta = p.IdPregunta
    LEFT JOIN Usuarios u ON r.IdUsuario = u.IdUsuario
    LEFT JOIN Respuestas rPadre ON r.IdRespuestaPadre = rPadre.IdRespuesta 
    LEFT JOIN Preguntas rp ON rPadre.IdPregunta = rp.IdPregunta
    WHERE r.IdPregunta = @IdPregunta AND r.Estado = 1;
END;
GO

CREATE PROCEDURE ObtenerRespuestas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        r.IdRespuesta,
        r.Contenido,
        r.IdPregunta,
        r.IdUsuario,
        r.IdRespuestaPadre,
        r.FechaCreacion,
        r.Estado,
        p.Titulo AS PreguntaTitulo,         
        u.Usuario AS Usuario,                
        u.Nombre AS UsuarioNombre,            
        r.IdRespuestaPadre AS RespuestaPadreId, 
        rp.Contenido AS RespuestaPadreContenido
    FROM Respuestas r
    LEFT JOIN Preguntas p ON r.IdPregunta = p.IdPregunta
    LEFT JOIN Usuarios u ON r.IdUsuario = u.IdUsuario
    LEFT JOIN Respuestas rp ON r.IdRespuestaPadre = rp.IdRespuesta
    WHERE r.Estado = 1;
END;

GO

CREATE PROCEDURE ObtenerPreguntasActivas
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.IdPregunta,
        p.Titulo,
        p.Contenido,
        p.IdUsuario,
        p.FechaCreacion,
        p.EstaCerrada,
        p.Estado          
    FROM Preguntas p
   
    WHERE p.Estado = 1; 
END;
GO

CREATE PROCEDURE CrearUsuario
    @Usuario NVARCHAR(50),
    @Contrasena NVARCHAR(100),
    @Nombre NVARCHAR(50),
    @Apellido NVARCHAR(50),
    @UrlFoto NVARCHAR(255) = NULL,
    @Login NVARCHAR(100),
    @Estado TINYINT  = 1, 
    @FechaRegistro DATETIME = NULL 
AS
BEGIN
    SET NOCOUNT ON;

   
    IF EXISTS (SELECT 1 FROM Usuarios WHERE Login = @Login)
    BEGIN
        RAISERROR('El usuario ya existe.', 16, 1);
        RETURN;
    END

  
    IF @FechaRegistro IS NULL
        SET @FechaRegistro = GETDATE();

    INSERT INTO Usuarios (Usuario, Contrasena, Nombre, Apellido, UrlFoto, Estado, Login, FechaRegistro)
    VALUES (@Usuario, @Contrasena, @Nombre, @Apellido, @UrlFoto, @Estado, @Login, @FechaRegistro);

    SELECT SCOPE_IDENTITY() AS IdUsuario;
END;
GO

CREATE PROCEDURE EditarEstadoPregunta
    @IdPregunta INT
AS
BEGIN
    SET NOCOUNT ON;

  
    UPDATE Preguntas
    SET EstaCerrada = 1
    WHERE IdPregunta = @IdPregunta
      AND (EstaCerrada = 0 OR EstaCerrada IS NULL); 

   
    IF @@ROWCOUNT = 0
    BEGIN
        PRINT 'No se actualiz� ninguna fila. Verifique que el IdPregunta sea v�lido y que el estado actual sea falso o nulo.';
    END
END;

GO

CREATE PROCEDURE CrearFotoPregunta
    @IdPregunta INT,
    @UrlFoto NVARCHAR(255)
AS
BEGIN
    BEGIN TRY
        
        INSERT INTO FotosPreguntas (IdPregunta, UrlFoto)
        VALUES (@IdPregunta, @UrlFoto);

        -- Retornar el Id generado
        SELECT SCOPE_IDENTITY() AS IdFotoPregunta;
    END TRY
    BEGIN CATCH
      
        THROW;
    END CATCH
END;
GO


CREATE PROCEDURE ObtenerFotosPorIdPregunta
    @IdPregunta INT
AS
BEGIN
    BEGIN TRY
     
        SELECT IdFotoPregunta, IdPregunta, UrlFoto
        FROM FotosPreguntas
        WHERE IdPregunta = @IdPregunta;
    END TRY
    BEGIN CATCH
       
        THROW;
    END CATCH
END;
GO

CREATE PROCEDURE ObtenerPreguntasPorUsuario
    @IdUsuario INT
AS
BEGIN
    BEGIN TRY
      
        SELECT 
            p.IdPregunta, 
            p.Titulo, 
            p.Contenido, 
            p.IdUsuario, 
            p.EstaCerrada, 
            p.FechaCreacion, 
            p.Estado,
            fp.IdFotoPregunta, 
            fp.UrlFoto, 
            r.IdRespuesta, 
            r.Contenido AS RespuestaContenido, 
            r.FechaCreacion AS RespuestaFechaCreacion
        FROM Preguntas p
        LEFT JOIN FotosPreguntas fp ON fp.IdPregunta = p.IdPregunta
        LEFT JOIN Respuestas r ON r.IdPregunta = p.IdPregunta
        WHERE p.IdUsuario = @IdUsuario AND p.Estado = 1;
    END TRY
    BEGIN CATCH
       
        THROW;
    END CATCH
END;
GO
CREATE PROCEDURE CrearPregunta
    @IdUsuario INT,
    @Titulo NVARCHAR(255),
    @Contenido NVARCHAR(MAX)
AS
BEGIN
    BEGIN TRY
      
        INSERT INTO Preguntas (IdUsuario, Titulo, Contenido, FechaCreacion, Estado)
        VALUES (@IdUsuario, @Titulo, @Contenido, GETDATE(), 1);

      
        SELECT SCOPE_IDENTITY() AS IdPregunta;
    END TRY
    BEGIN CATCH
      
        THROW;
    END CATCH
END;
GO
CREATE PROCEDURE ObtenerFotosPreguntas
AS
BEGIN
    BEGIN TRY
      
        SELECT fp.IdFotoPregunta, fp.IdPregunta, fp.UrlFoto, p.Titulo AS TituloPregunta
        FROM FotosPreguntas fp
        INNER JOIN Preguntas p ON fp.IdPregunta = p.IdPregunta
        WHERE p.Estado = 1 
    END TRY
    BEGIN CATCH
        
        THROW;
    END CATCH
END;
GO



CREATE PROCEDURE ObtenerFotoPreguntaPorId
    @IdFotoPregunta INT
AS
BEGIN
    BEGIN TRY
        SELECT fp.IdFotoPregunta, fp.IdPregunta, fp.UrlFoto, p.Titulo AS TituloPregunta
        FROM FotosPreguntas fp
        INNER JOIN Preguntas p ON fp.IdPregunta = p.IdPregunta
        WHERE fp.IdFotoPregunta = @IdFotoPregunta
        AND p.Estado = 1; 
    END TRY
    BEGIN CATCH
       
        THROW;
    END CATCH
END;
GO



