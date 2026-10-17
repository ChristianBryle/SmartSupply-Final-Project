IF DB_ID('SmartSupplyDB') IS NULL
    CREATE DATABASE SmartSupplyDB;
GO

use SmartSupplyDB;
GO

IF OBJECT_ID('categories', 'U') IS NULL
CREATE TABLE categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL,
    description VARCHAR(255)
);

IF OBJECT_ID('products', 'U') IS NULL
CREATE TABLE products (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    product_name VARCHAR(100) NOT NULL,
    category_id INT,
    quantity INT NOT NULL DEFAULT 0,
    unit_price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    reorder_level INT NOT NULL DEFAULT 10,

    FOREIGN KEY (category_id)
        REFERENCES categories(category_id)

);

IF OBJECT_ID('Transactions_log', 'U') IS NULL
CREATE TABLE Transactions_log (
    transaction_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    transaction_type VARCHAR(10) NOT NULL
        CHECK (transaction_type IN ('Stock In', 'Stock Out')),
    quantity INT NOT NULL,
    transaction_date DATETIME DEFAULT GETDATE(),
    remarks VARCHAR(255),

    FOREIGN KEY (product_id)
        REFERENCES products(product_id)
);

IF OBJECT_ID('users', 'U') IS NULL
CREATE TABLE users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL
);
GO

-- Default login: admin / admin123 (SHA-256, lowercase hex)
DELETE FROM users;
INSERT INTO users (username, password_hash)
VALUES ('admin', LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2)));
GO
