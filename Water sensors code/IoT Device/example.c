/* Code adapted from dev-Kit to received data from arduino and post it
 * Copyright 2017-2018 Myriad Group AG
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
   http://www.apache.org/licenses/LICENSE-2.0
 */

#include <common.h>
#include <string.h>
#include <math.h>

#include <platform.h>
#include <gsm.h>
#include <pin_mux.h>
#include <leds.h>
#include <powerControl.h>
#include <client_api.h>
#include <base64_codec_transport.h>
#include <thingstream_transport.h>
#include <modem_transport.h>
#include <client_platform.h>
#include <log_client_transport.h>
#include <log_modem_transport.h>
#include <line_buffer_transport.h>
#include <gsm_uart_transport.h>
#include <debug_printf.h>

static uint8_t gsm_buffer[290];

/* This routine is called after an ASSERT() macro has failed.
 * @param line  the line number of the ASSERT() failure.
 * @param what  the text of the ASSERT() being checked.
 * @return loops indefinitely so does not return.
 */
static void AssertFail(int line, const char* what)
{
    debug_printf("Assert fail \"%s\" at line %d\n", what, line);
    Led_Flash(100, 0, 0, 500, 500, 10);
    Led_Background(100, 0, 0);
    while (true)
    {
        CheckForSysTick();
        __WFI(); /* Wait for Interrupt */
    }
}
#define ASSERT(truth)                                                        \
    do                                                                       \
    {                                                                        \
        int line = __LINE__;                                                 \
        if (!(truth))                                                        \
        {                                                                    \
            AssertFail(line, #truth);                                        \
        }                                                                    \
    } while(0)

static void dump_payload(uint8_t* payload, uint16_t payloadlen)
{
    DEBUGOUT("\"");
    while (payloadlen-- > 0)
    {
        uint8_t byte = *payload++;
        if (byte == '\"')
        {
            DEBUGOUT("\\\"");
        }
        else if ((byte >= ' ') && (byte <= '~'))
        {
            DEBUGOUT("%c", byte);
        }
        else if (byte == '\n')
        {
            DEBUGOUT("\n");
        }
        else
        {
            DEBUGOUT("[%02x]", byte);
        }
    }
    DEBUGOUT("\"\n");
}

static void subscribe_callback(void *cookie, Topic topic, QOS qos, uint8_t* payload, uint16_t payloadlen)
{
    DEBUGOUT("subscribe_callback: topic %d::%d ",
             topic.topicType, topic.topicId);
    dump_payload(payload, payloadlen);
}

#define THINGSTREAM_BUFFER_LENGTH 1024
static uint8_t thingstream_buffer[THINGSTREAM_BUFFER_LENGTH];


static void modem_callback(void *cookie, const char* response, uint16_t len)
{
    DEBUGOUT("modem_callback: ");
    dump_payload((uint8_t*)response, len);
}

int getFinalValue(int vals[]){
	double value = (pow(2,4)*vals[0])+(pow(2,3)*vals[1])+(pow(2,2)*vals[2])+(2*vals[3])+(1*vals[4]);
	return (int)value;
}

int main(void)
{
    Platform_init();
    DEBUGOUT("initialising\n");

    Platform_GsmPinInit();
    Platform_GsmEnable();
    gpio_pin_config_t gpioInConfig = { kGPIO_DigitalInput, 0 };
    GPIO_PinInit(GPIOD, 7, &gpioInConfig);
    GPIO_PinInit(GPIOD, 6, &gpioInConfig);
    GPIO_PinInit(GPIOD, 5, &gpioInConfig);
    GPIO_PinInit(GPIOD, 4, &gpioInConfig);
    GPIO_PinInit(GPIOC, 2, &gpioInConfig);
    int values[5];
    values[0]= GPIO_ReadPinInput(GPIOD, 7);
    values[1] = GPIO_ReadPinInput(GPIOD, 6);
    values[2] = GPIO_ReadPinInput(GPIOD, 5);
    values[3]= GPIO_ReadPinInput(GPIOD, 4);
    values[4] = GPIO_ReadPinInput(GPIOC, 2);
    DEBUGOUT("Value at pin 18 is");
    int outV = getFinalValue(values);
    char msg1[2];
    sprintf(msg1, "%u", outV);
    char a[] = "{\"Value\":\"xx\"}";
    a[10]= msg1[0];
    a[11]= msg1[1];
    Led_Background(100, 0, 100); /* magenta = waiting for gsm carrier */

    DEBUGOUT("creating Transport layers\n");
    Transport* transport = gsm_uart_transport_create();
    ASSERT(transport != NULL);
    transport = line_buffer_transport_create(transport,
                                          gsm_buffer, sizeof(gsm_buffer));
    ASSERT(transport != NULL);

#if (defined(DEBUG_LOG_MODEM) && (DEBUG_LOG_MODEM > 0))
    transport = log_modem_transport_create(transport, debug_printf,
                                           TLOG_TRACE | TLOG_TIME);
    ASSERT(transport != NULL);
#endif /* DEBUG_LOG_MODEM */

    Transport* modem = modem_transport_create(transport, 0);
    ASSERT(modem != NULL);
    Modem_set_modem_callback(modem, modem_callback, NULL);
    transport = base64_codec_create(modem);
    ASSERT(transport != NULL);
    transport = thingstream_transport_create(transport,
                                             thingstream_buffer,
                                             THINGSTREAM_BUFFER_LENGTH);
    ASSERT(transport != NULL);

#if (defined(DEBUG_LOG_CLIENT) && (DEBUG_LOG_CLIENT > 0))
    transport = log_client_transport_create(transport, debug_printf,
                                            TLOG_TRACE | TLOG_TIME);
    ASSERT(transport != NULL);
#endif /* DEBUG_LOG_CLIENT */

    Client* client = Client_create(transport, "OLO1VRHTMQCYVYNSWMUX");
    ASSERT(client != NULL);
    TransportResult tRes = Modem_send_line(modem, "AT+CIMI\n", 60000);
    DEBUGOUT("%s: Modem_send_line: %d\n", Platform_getTimeString(), tRes);
    ASSERT(tRes == TRANSPORT_SUCCESS);
    DEBUGOUT("connecting\n");
    ClientResult cr = Client_connect(client, true, NULL, NULL);
    DEBUGOUT("Client_connect => %d\n", cr);
    ASSERT(cr == CLIENT_SUCCESS);
    Led_Background(0, 0, 100); /* blue = connected */
    Client_set_subscribe_callback(client, subscribe_callback, NULL);
    Topic topic;
    const char* topicName1 = "/TestSend";
    DEBUGOUT("registering '%s'\n", topicName1);
    cr = Client_register(client, topicName1, &topic);
    ASSERT(cr == CLIENT_SUCCESS);
    DEBUGOUT("publishing\n");
    cr = Client_publish(client, topic, MQTT_QOS1, false,
                       (uint8_t*)a, strlen(a), NULL);
    ASSERT(cr == CLIENT_SUCCESS);
    uint32_t waitSeconds = 3 * 60;
    DEBUGOUT("waiting %d seconds\n", waitSeconds);
    cr = Client_run(client, waitSeconds * 1000);
    ASSERT(cr == CLIENT_SUCCESS);
    cr = Client_disconnect(client, 0);
    ASSERT(cr == CLIENT_SUCCESS);
    Led_Background(0, 100, 0);  /* green = disconnected */

    transport->shutdown(transport);

    DEBUGOUT("done\n");

    while (1)
    {
        Platform_sleep(60 * 60 * 1000);
    }
}
