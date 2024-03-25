# Push Messages
The Push Messages module enables marketers to send custom notifications to selected customers or organizations within the Virto Commerce platform.

## Overview
The Push Messages module provides a seamless solution for Marketers to customize and send notifications to fronted applications for specific customers or organizations. With options to tailor the Message of the notification, Marketers can effectively communicate with targeted groups. Additionally, the module offers features for displaying delivery status for notifications.

![image](https://github.com/VirtoCommerce/vc-module-push-messages/assets/7639413/cc931ef6-3aca-4b25-b4c7-177ff9157433)

## Features
* Send a Short Message in HTML format. Max length: 1024.
* Select multiple customers or organizations for notification delivery
* Check Delivery status display per customer
* Notification Preview (Soon)
* Templates (Soon)
* Extend message with Subjet and Full Message (Soon)
* Add attachments (Soon)
* Reporting (Soon)
* More Notifications Channels: Browser Push Notifications, SMS, Email, etc. (Soon)

## Screenshots
![image](https://github.com/VirtoCommerce/vc-module-push-messages/assets/7639413/28ceecd6-1ada-42b0-a778-38f424a836a1)

![image](https://github.com/VirtoCommerce/vc-module-push-messages/assets/7639413/564f9efd-421e-47c0-84d9-72f2717597cd)

![image](https://github.com/VirtoCommerce/vc-module-push-messages/assets/7639413/506ed18c-ff82-4f47-9dd7-2623de19875c)

![image](https://github.com/VirtoCommerce/vc-module-push-messages/assets/7639413/780a7014-a8fa-43e1-9b87-46bd15b4a16f)


## XAPI Specification

### Subscriptions
```js
subscription pushMessageCreated {
  pushMessageCreated {
    id
    shortMessage
    createdDate
    isRead
  }
}
```
---
### Query
```js
{
  pushMessages (unreadOnly: true, cultureName: "en-Us") {
    unreadCount
    items {
      id
      shortMessage
      createdDate
      isRead
    }
  }
}
```
### Mutations
```js
mutation clearAllPushMessages{
    clearAllPushMessages
}
```

```js
mutation markAllPushMessagesRead{
    markAllPushMessagesRead
}
```

```js
mutation markAllPushMessagesUnread{
    markAllPushMessagesUnread
}
```

```js
mutation markPushMessageRead($command: InputMarkPushMessageReadType!) {
    markPushMessageRead(command: $command)
}
```
```js
{
  "command": {
    "messageId": "123"
  }
}
```

```js
mutation markPushMessageUnread($command: InputMarkPushMessageUnreadType!) {
    markPushMessageUnread(command: $command)
}
```
```js
{
  "command": {
    "messageId": "123"
  }
}
```
