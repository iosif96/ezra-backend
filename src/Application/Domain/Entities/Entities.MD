# Ezra — Domain Model

> All entities carry `Id` (PK) and `CreatedAt` by default, they inherit the AuditableEntity.

---

## Airport
| Field | Type | Notes |
|---|---|---|
| Name | String | |
| IataCode | String(3) | Unique |
| IcaoCode | String(4) | Unique |
| City | String | |
| CountryCode | String(2) | ISO 3166-1 alpha-2 |
| DefaultLanguage | String | BCP 47, e.g. `en`, `fr`, `ar` |
| PromptInformation | Text | Injected into Ezra's system prompt |

---

## Terminal
| Field | Type | Notes |
|---|---|---|
| AirportId | FK → Airport | |
| Code | String | e.g. `2E` |
| PromptInformation | Text | |

---

## Gate
| Field | Type | Notes |
|---|---|---|
| TerminalId | FK → Terminal | |
| Code | String | e.g. `E42` |
| PromptInformation | Text | |

---

## Flight
| Field | Type | Notes |
|---|---|---|
| Date | Date | Scheduled departure date |
| Number | String | e.g. `1680` |
| IataCode | String | e.g. `AF1680` |
| IcaoCode | String | e.g. `AFR1680` |
| Airline | String | e.g. `Air France` |
| Status | Enum | `scheduled` · `estimated` · `active` · `landed` · `arrived` · `departed` · `cancelled` · `diverted` · `unknown` |
| FlightSource | Enum | Source data provider |

---

## FlightMovement
| Field | Type | Notes |
|---|---|---|
| FlightId | FK → Flight | |
| Type | Enum | `departure` · `arrival` |
| AirportId | FK → Airport | |
| TerminalId | FK → Terminal? | |
| GateId | FK → Gate? | |
| BoardingOn | Timestamp? | Departure movements only |
| GateCloseOn | Timestamp? | Departure movements only |
| ScheduledOn | Timestamp | |
| EstimatedOn | Timestamp? | Set when delay is detected |
| ActualOn | Timestamp? | Set after the event occurs |

---

## BoardingPass
| Field | Type | Notes |
|---|---|---|
| FlightId | FK → Flight | |
| Code | String | Barcode / PNR |
| Seat | String? | e.g. `14C` |
| IdentityId | FK → Identity | |

---

## Identity
| Field | Type | Notes |
|---|---|---|
| PassengerName | String? | Resolved from boarding pass scan |
| *(BoardingPasses)* | BoardingPass[] | Navigational |
| *(Conversations)* | Conversation[] | Navigational |

---

## Staff
| Field | Type | Notes |
|---|---|---|
| AirportId | FK → Airport | |
| Name | String | |
| PhoneNumber | String | |

---

## Touchpoint
| Field | Type | Notes |
|---|---|---|
| AirportId | FK → Airport | |
| TerminalId | FK → Terminal? | |
| GateId | FK → Gate? | |
| Label | String | e.g. `Left pillar, Gate E42` |
| Latitude | Float | |
| Longitude | Float | |

---

## TouchpointScan
| Field | Type | Notes |
|---|---|---|
| TouchpointId | FK → Touchpoint | |
| ChannelType | Enum | `whatsapp` · `messenger` · `telegram` |

---

## Merchant
| Field | Type | Notes |
|---|---|---|
| TerminalId | FK → Terminal | |
| Name | String | |
| IsAirside | Boolean | Passenger must have cleared security |
| Latitude | Float | |
| Longitude | Float | |
| PromptInformation | Text | How Ezra should pitch this merchant |

---

## Conversation
| Field | Type | Notes |
|---|---|---|
| ChannelType | Enum | `whatsapp` · `messenger` · `telegram` |
| ChannelId | String | Phone number or platform user ID |
| IdentityId | FK → Identity? | Null until boarding pass is resolved |
| LastMessageOn | Timestamp | |
| *(Messages)* | Message[] | Navigational |

---

## Message
| Field | Type | Notes |
|---|---|---|
| ConversationId | FK → Conversation | |
| Type | Enum | `system` · `assistant` · `user` |
| MediaType | Enum | `text` · `voice` · `image` · `document` |
| MediaUrl | String? | For voice, image, document |
| Content | Text? | Transcription if voice · OCR extract if image |
| Model | String? | LLM model used, null for user messages |
| InputTokens | Int? | Null for user messages |
| OutputTokens | Int? | Null for user messages |
| StressIndex | Float? | `0.0–1.0` · evaluated by LLM · null for system messages |

---

## Request
| Field | Type | Notes |
|---|---|---|
| ConversationId | FK → Conversation | |
| Type | Enum | `SpecialNeedsAssistance` · `Handoff` · `Emergency` |
| Content | Text? | Passenger's own words |
| StaffId | FK → Staff? | Assigned after creation |
| Status | Enum | `requested` · `confirmed` · `cancelled` · `resolved` |

---

## Event
> Flight state changes and other triggers for mass message push.

| Field | Type | Notes |
|---|---|---|
| Type | Enum | `boarding_open` · `gate_changed` · `delay` · `cancellation` · `diversion` · `custom` |
| FlightId | FK → Flight? | Null for non-flight events |
| ScheduledOn | Timestamp? | Null if triggered immediately on creation |
| Content | Text | The message body to be sent |
| *(Notifications)* | Message[] | Fan-out records — messages with `IsProactive = true` |

---

## Feedback
| Field | Type | Notes |
|---|---|---|
| ConversationId | FK → Conversation | |
| FlightId | FK → Flight | |
| Rating | Int | `1–5` |
| Content | Text? | |
| SentimentScore | Float? | `-1.0 to 1.0` · auto-evaluated from content |
