-- MySQL 8.0 – GranjaTech (modelo para EER)
-- Ajuste o schema conforme precisar
CREATE DATABASE IF NOT EXISTS granjatech CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
USE granjatech;

-- 1) Núcleo de usuários
CREATE TABLE Perfis (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Nome VARCHAR(255) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE Usuarios (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Codigo VARCHAR(255) NOT NULL,
  Nome   VARCHAR(255) NOT NULL,
  Email  VARCHAR(255) NOT NULL,
  SenhaHash VARCHAR(255) NOT NULL,
  PerfilId INT NOT NULL,
  CONSTRAINT FK_Usuarios_Perfis FOREIGN KEY (PerfilId) REFERENCES Perfis(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- 2) Auditoria
CREATE TABLE LogsAuditoria (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  `Timestamp` DATETIME(6) NOT NULL,
  UsuarioId INT NOT NULL,
  UsuarioEmail TEXT NOT NULL,
  Acao TEXT NOT NULL,
  Detalhes TEXT NOT NULL
) ENGINE=InnoDB;

-- 3) Granjas e Lotes
CREATE TABLE Granjas (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Codigo VARCHAR(255) NOT NULL,
  Nome   VARCHAR(255) NOT NULL,
  Localizacao TEXT,
  UsuarioId INT NOT NULL,
  CONSTRAINT FK_Granjas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
  INDEX IX_Granjas_UsuarioId (UsuarioId)
) ENGINE=InnoDB;

CREATE TABLE Lotes (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Codigo        VARCHAR(50) NOT NULL,
  Identificador VARCHAR(100) NOT NULL,
  DataEntrada   DATETIME(6) NOT NULL,
  DataSaida     DATETIME(6) NULL,
  QuantidadeAvesInicial INT NOT NULL,
  GranjaId INT NOT NULL,

  -- campos extras do script
  AreaGalpao DECIMAL(18,4) NULL,
  DataAbatePrevista DATETIME(6) NULL,
  DataAtualizacao   DATETIME(6) NULL,
  DataCriacao       DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  Linhagem          VARCHAR(100) NULL,
  Observacoes       VARCHAR(1000) NULL,
  OrigemPintinhos   VARCHAR(200) NULL,
  QuantidadeAvesAtual INT NOT NULL DEFAULT 0,
  Status            VARCHAR(50) NOT NULL DEFAULT '',

  CONSTRAINT FK_Lotes_Granjas FOREIGN KEY (GranjaId) REFERENCES Granjas(Id) ON DELETE CASCADE,
  INDEX IX_Lotes_GranjaId (GranjaId)
) ENGINE=InnoDB;

-- 4) Produtos
CREATE TABLE Produtos (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Nome VARCHAR(255) NOT NULL,
  Tipo VARCHAR(255) NOT NULL,
  Quantidade DECIMAL(18,4) NOT NULL,
  UnidadeDeMedida VARCHAR(100) NOT NULL,
  GranjaId INT NOT NULL,
  CONSTRAINT FK_Produtos_Granjas FOREIGN KEY (GranjaId) REFERENCES Granjas(Id) ON DELETE CASCADE,
  INDEX IX_Produtos_GranjaId (GranjaId)
) ENGINE=InnoDB;

-- 5) Transações financeiras e vínculo produtor
CREATE TABLE TransacoesFinanceiras (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Descricao TEXT NOT NULL,
  Valor DECIMAL(18,2) NOT NULL,
  Tipo  VARCHAR(50) NOT NULL,
  Data  DATETIME(6) NOT NULL,
  TimestampCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UsuarioId INT NOT NULL,
  LoteId INT NULL,
  CONSTRAINT FK_TransFin_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
  CONSTRAINT FK_TransFin_Lotes    FOREIGN KEY (LoteId)    REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_TransacoesFinanceiras_UsuarioId (UsuarioId),
  INDEX IX_TransacoesFinanceiras_LoteId (LoteId)
) ENGINE=InnoDB;

CREATE TABLE FinanceiroProdutor (
  FinanceiroId INT NOT NULL,
  ProdutorId   INT NOT NULL,
  PRIMARY KEY (FinanceiroId, ProdutorId),
  CONSTRAINT FK_FinanceiroProdutor_Financeiro FOREIGN KEY (FinanceiroId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
  CONSTRAINT FK_FinanceiroProdutor_Produtor   FOREIGN KEY (ProdutorId)   REFERENCES Usuarios(Id) ON DELETE CASCADE,
  INDEX IX_FinanceiroProdutor_ProdutorId (ProdutorId)
) ENGINE=InnoDB;

-- 6) Sensores e leituras
CREATE TABLE Sensores (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Tipo VARCHAR(100) NOT NULL,
  IdentificadorUnico VARCHAR(255) NOT NULL,
  GranjaId INT NOT NULL,
  CONSTRAINT FK_Sensores_Granjas FOREIGN KEY (GranjaId) REFERENCES Granjas(Id) ON DELETE CASCADE,
  INDEX IX_Sensores_GranjaId (GranjaId)
) ENGINE=InnoDB;

CREATE TABLE LeiturasSensores (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Valor DECIMAL(18,4) NOT NULL,
  `Timestamp` DATETIME(6) NOT NULL,
  SensorId INT NOT NULL,
  CONSTRAINT FK_Leituras_Sensores FOREIGN KEY (SensorId) REFERENCES Sensores(Id) ON DELETE CASCADE,
  INDEX IX_LeiturasSensores_SensorId (SensorId)
) ENGINE=InnoDB;

-- 7) Consumos
CREATE TABLE ConsumosAgua (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  LoteId INT NOT NULL,
  Data DATETIME(6) NOT NULL,
  QuantidadeLitros DECIMAL(10,3) NOT NULL,
  AvesVivas INT NOT NULL,
  TemperaturaAmbiente DECIMAL(18,4) NULL,
  Observacoes VARCHAR(500) NULL,
  DataCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  CONSTRAINT FK_ConsAgua_Lotes FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_ConsumosAgua_LoteId_Data (LoteId, Data)
) ENGINE=InnoDB;

CREATE TABLE ConsumosRacao (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  LoteId INT NOT NULL,
  Data DATETIME(6) NOT NULL,
  QuantidadeKg DECIMAL(10,3) NOT NULL,
  TipoRacao VARCHAR(50) NOT NULL,
  AvesVivas INT NOT NULL,
  Observacoes VARCHAR(500) NULL,
  DataCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  CONSTRAINT FK_ConsRacao_Lotes FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_ConsumosRacao_LoteId_Data (LoteId, Data)
) ENGINE=InnoDB;

-- 8) Sanitário
CREATE TABLE EventosSanitarios (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  LoteId INT NOT NULL,
  Data DATETIME(6) NOT NULL,
  TipoEvento VARCHAR(50) NOT NULL,
  Produto VARCHAR(200) NOT NULL,
  LoteProduto VARCHAR(100) NULL,
  Dosagem VARCHAR(100) NULL,
  ViaAdministracao VARCHAR(50) NULL,
  AvesTratadas INT NULL,
  DuracaoTratamentoDias INT NULL,
  PeriodoCarenciaDias INT NULL,
  ResponsavelAplicacao VARCHAR(200) NULL,
  Sintomas VARCHAR(1000) NULL,
  Observacoes VARCHAR(1000) NULL,
  Custo DECIMAL(18,2) NULL,
  DataCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  CONSTRAINT FK_Eventos_Lotes FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_EventosSanitarios_LoteId (LoteId)
) ENGINE=InnoDB;

-- 9) Qualidade do ar
CREATE TABLE MedicoesQualidadeAr (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  LoteId INT NOT NULL,
  DataHora DATETIME(6) NOT NULL,
  NH3_ppm DECIMAL(6,2) NULL,
  CO2_ppm DECIMAL(8,2) NULL,
  O2_percentual DECIMAL(5,2) NULL,
  VelocidadeAr_ms DECIMAL(10,2) NULL,
  Luminosidade_lux DECIMAL(18,4) NULL,
  TemperaturaAr DECIMAL(10,2) NULL,
  UmidadeRelativa DECIMAL(10,2) NULL,
  LocalMedicao VARCHAR(100) NULL,
  EquipamentoMedicao VARCHAR(200) NULL,
  Observacoes VARCHAR(500) NULL,
  DataCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  CONSTRAINT FK_QAr_Lotes FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_MedicoesQualidadeAr_LoteId (LoteId)
) ENGINE=InnoDB;

-- 10) Pesagens
CREATE TABLE PesagensSemanais (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  LoteId INT NOT NULL,
  DataPesagem DATETIME(6) NOT NULL,
  IdadeDias INT NOT NULL,
  SemanaVida INT NOT NULL,
  PesoMedioGramas DECIMAL(8,2) NOT NULL,
  QuantidadeAmostrada INT NOT NULL,
  PesoMinimo DECIMAL(18,4) NULL,
  PesoMaximo DECIMAL(18,4) NULL,
  DesvioPadrao DECIMAL(18,4) NULL,
  CoeficienteVariacao DECIMAL(18,4) NULL,
  GanhoSemanal DECIMAL(18,4) NULL,
  Observacoes VARCHAR(500) NULL,
  DataCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  CONSTRAINT FK_Pesagens_Lotes FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_PesagensSemanais_LoteId_SemanaVida (LoteId, SemanaVida)
) ENGINE=InnoDB;

-- 11) Abate
CREATE TABLE RegistrosAbate (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  LoteId INT NOT NULL,
  DataAbate DATETIME(6) NOT NULL,
  DataAbatePrevista DATETIME(6) NULL,
  IdadeAbateDias INT NOT NULL,
  QuantidadeEnviada INT NOT NULL,
  PesoVivoTotalKg DECIMAL(10,3) NOT NULL,
  PesoCarcacaTotalKg DECIMAL(18,3) NULL,
  AvesCondenadas INT NULL,
  MotivoCondenacoes VARCHAR(1000) NULL,
  PesoCondenadoKg DECIMAL(18,3) NULL,
  FrigorificoDestino VARCHAR(200) NULL,
  Transportadora VARCHAR(200) NULL,
  ValorPorKg DECIMAL(18,2) NULL,
  ValorTotalRecebido DECIMAL(18,2) NULL,
  Observacoes VARCHAR(1000) NULL,
  DataCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  CONSTRAINT FK_Abate_Lotes FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_RegistrosAbate_LoteId (LoteId)
) ENGINE=InnoDB;

-- 12) Mortalidade (estrutura final após alterações do script)
CREATE TABLE RegistrosMortalidade (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  LoteId INT NOT NULL,
  Data DATETIME(6) NOT NULL,
  Quantidade INT NOT NULL,
  Motivo VARCHAR(200) NULL,
  Setor VARCHAR(100) NULL,
  Observacoes VARCHAR(1000) NULL,
  ResponsavelRegistro VARCHAR(200) NULL,
  DataCriacao DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  CONSTRAINT FK_Mortalidade_Lotes FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
  INDEX IX_RegistrosMortalidade_LoteId_Data (LoteId, Data)
) ENGINE=InnoDB;

-- Seeds mínimos (opcional)
INSERT INTO Perfis (Id, Nome) VALUES (1,'Administrador'), (2,'Produtor'), (3,'Financeiro')
  ON DUPLICATE KEY UPDATE Nome=VALUES(Nome);

INSERT INTO Usuarios (Id, Codigo, Email, Nome, PerfilId, SenhaHash)
VALUES (1,'ADM-001','admin@admin.com','Admin Padrão',1,'$2a$11$Y.7g.3s5B5B5B5B5B5B.u5n5n5n5n5n5n5n5n5n5n5n5n5n5n5n5')
ON DUPLICATE KEY UPDATE Nome=VALUES(Nome), Email=VALUES(Email), PerfilId=VALUES(PerfilId);
