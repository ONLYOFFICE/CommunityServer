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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ASC.Common.Logging;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ASC.TelegramService.Core
{
    public class TelegramCommand
    {
        public string CommandName { get; private set; }
        public string[] Args { get; private set; }

        public Message Message { get; private set; }
        public User User { get { return Message.From; } }
        public Chat Chat { get { return Message.Chat; } }

        public TelegramCommand(Message msg, string cmdName, string[] args = null)
        {
            Message = msg;
            CommandName = cmdName;
            Args = args;
        }
    }

    public class CommandModule
    {
        private readonly ILog log;

        private static readonly Regex cmdReg = new Regex(@"^\/([^\s]+)\s?(.*)");
        private static readonly Regex argsReg = new Regex(@"[^""\s]\S*|"".+?""");

        private readonly Dictionary<string, MethodInfo> commands = new Dictionary<string, MethodInfo>();
        private readonly Dictionary<string, Type> contexts = new Dictionary<string, Type>();
        private readonly Dictionary<Type, ParamParser> parsers = new Dictionary<Type, ParamParser>();

        public CommandModule(ILog log)
        {
            this.log = log;

            var assembly = Assembly.GetExecutingAssembly();

            foreach (var t in assembly.GetExportedTypes())
            {
                if (t.IsAbstract) continue;

                if (t.IsSubclassOf(typeof(CommandContext)))
                {
                    foreach (var method in t.GetRuntimeMethods())
                    {
                        if (method.IsPublic && Attribute.IsDefined(method, typeof(CommandAttribute)))
                        {
                            var attr = method.GetCustomAttribute<CommandAttribute>();
                            commands.Add(attr.Name, method);
                            contexts.Add(attr.Name, t);
                        }
                    }
                }

                if (t.IsSubclassOf(typeof(ParamParser)) && Attribute.IsDefined(t, typeof(ParamParserAttribute)))
                {
                    parsers.Add(t.GetCustomAttribute<ParamParserAttribute>().Type, (ParamParser)Activator.CreateInstance(t));
                }
            }
        }

        private TelegramCommand ParseCommand(Message msg)
        {
            var reg = cmdReg.Match(msg.Text);
            var args = argsReg.Matches(reg.Groups[2].Value);

            return new TelegramCommand(msg, reg.Groups[1].Value.ToLowerInvariant(), args.Count > 0 ? args.Cast<Match>().Select(a => a.Value).ToArray() : null);
        }

        private object[] ParseParams(MethodInfo cmd, string[] args)
        {
            List<object> parsedParams = new List<object>();

            var cmdArgs = cmd.GetParameters();

            if (cmdArgs.Any() && args == null || cmdArgs.Count() != args.Count()) throw new Exception("Wrong parameters count");

            for (var i = 0; i < cmdArgs.Count(); i++)
            {
                var type = cmdArgs[i].ParameterType;

                if (type == typeof(string))
                {
                    parsedParams.Add(args[i]);
                    continue;
                }

                if (!parsers.ContainsKey(type)) throw new Exception(string.Format("No parser found for type '{0}'", type));

                parsedParams.Add(parsers[cmdArgs[i].ParameterType].FromString(args[i]));
            }

            return parsedParams.ToArray();
        }

        public async Task HandleCommand(Message msg, TelegramBotClient client, int tenantId)
        {
            try
            {
                var cmd = ParseCommand(msg);

                if (!commands.ContainsKey(cmd.CommandName)) throw new Exception(string.Format("No handler found for command '{0}'", cmd.CommandName));

                var command = commands[cmd.CommandName];
                var context = (CommandContext)Activator.CreateInstance(contexts[cmd.CommandName]);

                var param = ParseParams(command, cmd.Args);

                context.Context = cmd;
                context.Client = client;
                context.TenantId = tenantId;
                await Task.FromResult(command.Invoke(context, param));
            }
            catch (Exception ex)
            {
                log.DebugFormat("Couldn't handle ({0}) message ({1})", msg.Text, ex.Message);
            }
        }
    }

    public abstract class CommandContext
    {
        public ITelegramBotClient Client { get; set; }
        public TelegramCommand Context { get; set; }
        public int TenantId { get; set; }

        protected async Task ReplyAsync(string message)
        {
            await Client.SendTextMessageAsync(Context.Chat, message);
        }
    }

    public abstract class ParamParser
    {
        protected Type type;

        protected ParamParser(Type type)
        {
            this.type = type;
        }

        public abstract object FromString(string arg);
        public abstract string ToString(object arg);
    }

    public abstract class ParamParser<T> : ParamParser
    {
        public ParamParser() : base(typeof(T)) { }

        public override abstract object FromString(string arg);
        public override abstract string ToString(object arg);
    }
}
