/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;

namespace ASC.Data.Backup.Tasks
{
    public abstract class ProgressTask : IProgressTask
    {
        private int _progress;
        private int _stepsCount;
        private int _stepsCompleted;

        public abstract void Run();

        protected void InitProgress(int stepsCount)
        {
            _stepsCount = stepsCount;
            _stepsCompleted = 0;
            _progress = 0;
        }

        protected void RunSubtask(IProgressTask task)
        {
            task.Message += (sender, args) => InvokeMessage(args.Reason, args.Message);
            task.ProgressChanged += (sender, args) => SetStepProgress(args.Progress);
            task.Run();
        }

        protected void SetProgress(int progress)
        {
            if (_progress != progress)
            {
                _progress = progress;
                InvokeProgressChanged(_progress);
            }
        }

        protected void SetStepProgress(int progress)
        {
            SetProgress((int)((_stepsCompleted*100 + progress)/(double)_stepsCount));
            
            if (progress == 100)
            {
                _stepsCompleted++;
            }
        }

        protected void SetStepCompleted()
        {
            SetProgress((int)((++_stepsCompleted * 100) / (double)_stepsCount));
        }

        #region events

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        public event EventHandler<MessageEventArgs> Message; 

        protected void InvokeProgressChanged(int progress)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, new ProgressChangedEventArgs(progress));
            }
        }

        protected void InvokeInfo(string messageFormat, params object[] args)
        {
            InvokeMessage(MessageReason.Info, messageFormat, args);
        }

        protected void InvokeWarning(string messageFormat, params object[] args)
        {
            InvokeMessage(MessageReason.Warning, messageFormat, args);
        }

        protected void InvokeMessage(MessageReason reason, string messageFormat, params object[] args)
        {
            if (args.Length > 0)
            {
                messageFormat = string.Format(messageFormat, args);
            }
            if (Message != null)
            {
                Message(this, new MessageEventArgs(messageFormat, reason));
            }
        }

        #endregion
    }
}
