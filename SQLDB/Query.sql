create database SmartSupplyDB;

use SmartSupplyDB;

CREATE TABLE categories (
    category_id      INT IDENTITY(1,1) PRIMARY KEY,
    category_name    VARCHAR(100) NOT NULL,
    description      VARCHAR(255)
);

CREATE TABLE products (
    product_id       INT IDENTITY(1,1) PRIMARY KEY,
    product_name     VARCHAR(100) NOT NULL,
    category_id      INT,
    quantity         INT DEFAULT 0,
    unit_price       DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    reorder_level    INT NOT NULL DEFAULT 10,

    FOREIGN KEY (category_id)
        REFERENCES categories(category_id),

);

CREATE TABLE Transactions_log (
    transaction_id    INT IDENTITY(1,1) PRIMARY KEY,
    product_id        INT NOT NULL,
    transaction_type  VARCHAR(10) NOT NULL CHECK (transaction_type IN ('Stock In', 'Stock Out')),
    quantity          INT NOT NULL,
    transaction_date  DATETIME DEFAULT GETDATE(),
    remarks           VARCHAR(255),

    FOREIGN KEY (product_id)
        REFERENCES products(product_id)
);

drop table quotations;

CREATE TABLE quotations (
    quotation_id      INT IDENTITY(1,1) PRIMARY KEY,
    quotation_name     VARCHAR(100) NOT NULL,
    quotation_number  AS ('QT-' + RIGHT('000000' + CAST(quotation_id AS VARCHAR(10)), 6)) PERSISTED,
    created_by        INT NOT NULL,                 
    quotation_date    DATE NOT NULL DEFAULT GETDATE(),
 
    CONSTRAINT FK_quotations_users
        FOREIGN KEY (created_by) REFERENCES users(user_id),
);
GO

CREATE TABLE quotation_items (
    quotation_item_id INT IDENTITY(1,1) PRIMARY KEY,
    quotation_id      INT NOT NULL,
    product_id        INT NOT NULL,
    quantity          INT NOT NULL CHECK (quantity > 0),
    unit_price        DECIMAL(10,2) NOT NULL CHECK (unit_price >= 0),
    total_price       AS (quantity * unit_price) PERSISTED,
 
    CONSTRAINT FK_qitems_quotations
        FOREIGN KEY (quotation_id) REFERENCES quotations(quotation_id) ON DELETE CASCADE,
    CONSTRAINT FK_qitems_products
        FOREIGN KEY (product_id) REFERENCES products(product_id),
    CONSTRAINT UQ_qitems_product_per_quote
        UNIQUE (quotation_id, product_id)
);
GO

CREATE TABLE users (
    user_id INT IDENTiTY(1,1) PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL
);

-- Username: user Password: user
INSERT INTO users (username, password_hash)
VALUES ('user', LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'user'), 2)));