Для запуска мобильной версии необходимо:

1. Настроить виртуальный каталог /asc/mobile на данный сайт
ВАЖНО: данный сайт должен быть подкаталогом студии asc, т.к. мобильная версия использует конфигурационные файлы от студии.
Отлаживать можно, запуская сайт на Use local IIS Server: http://localhost/asc/mobile
(Данные настройки выставлены в проекте по умолчанию)

2. Установить isapi фльтр для mvc:
Свойства виртуальной директории mobile -> Configuration -> Mappings -> Кнопка Add
Executable: C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll
Extension: .*
All Verbs: true
Check that file exists: false

3. Разрешить Integrated Windows Authentication:
Свойства виртуальной директории mobile -> Directory Security -> Edit -> enable Integrated Windows Authentication
