create database SmartSupplyDB;

use SmartSupplyDB;

CREATE TABLE categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name VARCHAR(100) NOT NULL,
    description VARCHAR(255)
);

CREATE TABLE products (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    product_name VARCHAR(100) NOT NULL,
    category_id INT,
    quantity INT NOT NULL DEFAULT 0,
    unit_price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    reorder_level INT NOT NULL DEFAULT 10,

    FOREIGN KEY (category_id)
        REFERENCES categories(category_id),

);

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

CREATE TABLE users (
    user_id INT IDENTiTY(1,1) PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL
);
