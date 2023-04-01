# RemoteClient
SSH/RDP 客户端账户管理工具。
用户账号密码存储于liteDb中，通过调用putty, winscp, mstsc实现客户端连接，也可以自行配置客户端。

使用AES加密用户名，密码和地址。
CipherMode.CBC，PaddingMode.PKCS7

如何使用：
配置AppSettings.json，启动页面输入Key和IV。IV偏移量可以不填，默认为“0123456789abcdef”。
