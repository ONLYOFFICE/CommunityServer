/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


module.exports = (voip) => {
    const router = require('express').Router();

    router
        .post("/enqueue", (req, res) => {
            const { numberId, callId, agent } = req.body;
            voip.enqueue(numberId, callId, agent);
            res.end();
        })
        .post("/incoming", (req, res) => {
            const { callId, agent } = req.body;
            voip.incoming(callId, agent);
            res.end();
        })
        .post("/miss", (req, res) => {
            const { numberId, callId, agent } = req.body;
            voip.miss(numberId, callId, agent);
            res.end();
        })
        .post("/getAgent", (req, res) => {
            const { numberId, contactsResponsibles } = req.body;
            const result = voip.getAgent(numberId, contactsResponsibles);
            res.send(JSON.stringify({item1: result.result, item2: result.isAnyNotOffline}));
        })
        .post("/reload", (req, res) => {
            const { numberRoom, agentId } = req.body;
            voip.reload(numberRoom, agentId);
            res.end();
        });

    return router;
}