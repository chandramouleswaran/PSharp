﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;
using Microsoft.PSharp.ReliableServices;
using Microsoft.PSharp.ReliableServices.Utilities;
using Microsoft.PSharp.ReliableServices.Timers;
using Microsoft.ServiceFabric.Data;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Data.Collections;

namespace TailSpin
{
	/*
	 * Each subscriber registers himself/herself, starts a survey, and when the survey completes, stores the result reliably.
	 */
	class Subscriber : ReliableStateMachine
	{
		#region fields

		/// <summary>
		/// Handle to the main TailSpinCore machine.
		/// </summary>
		ReliableRegister<IRsmId> TailSpinCoreMachine;

		/// <summary>
		/// Reliably store the unique id assigned to the subscriber by TailSpinCore.
		/// </summary>
		ReliableRegister<int> SubscriberId;

		/// <summary>
		/// Reliably store the results of all the surveys.
		/// </summary>
		IReliableDictionary<int, int> SurveyResponses;

		#endregion

		#region internal events

		/// <summary>
		/// Start off a survey
		/// </summary>
		[DataContract]
		class StartSurveyEvent : Event { }

		#endregion

		#region states

		[Start]
		[OnEntry(nameof(Initialize))]
		[OnEventDoAction(typeof(RegistrationSuccessEvent), nameof(HandleRegistrationSuccess))]
		[OnEventDoAction(typeof(StartSurveyEvent), nameof(InitiateSurvey))]
		[OnEventDoAction(typeof(SurveyCreationSuccessEvent), nameof(HandleSuccssfulSurveyCreation))]
		[OnEventDoAction(typeof(SurveyResultsEvent), nameof(RecordSurveyResults))]
		class Init : MachineState { }

		#endregion

		#region handlers
		async Task Initialize()
		{
			SubscriberInitEvent e = (this.ReceivedEvent as SubscriberInitEvent);
			await TailSpinCoreMachine.Set(e.TailSpinCoreMachine);

			// Cache the handle to TailSpinCoreMachine
			var tsCore = await TailSpinCoreMachine.Get();

			// Register myself
			await this.ReliableSend(tsCore, new RegisterSubscriberEvent(this.ReliableId));
		}

		/// <summary>
		/// On successful registration, reliably store the subscriber id and start a survey.
		/// </summary>
		/// <returns></returns>
		async Task HandleRegistrationSuccess()
		{
			RegistrationSuccessEvent e = (this.ReceivedEvent as RegistrationSuccessEvent);
			this.Logger.WriteLine("Subscriber " + this.Id + " registered with id: " + e.SubscriberId);
			await SubscriberId.Set(e.SubscriberId);
			await this.ReliableSend(this.ReliableId, new StartSurveyEvent());
		}

		async Task InitiateSurvey()
		{
			var tsCore = await TailSpinCoreMachine.Get();
			int subscriberId = await SubscriberId.Get();

			await this.ReliableSend(tsCore, new CreateSurveyEvent(subscriberId));
		}

		/// <summary>
		/// Reliably store the id, generated by TailSpinCore, of the newly created survey.
		/// </summary>
		/// <returns></returns>
		async Task HandleSuccssfulSurveyCreation()
		{
			// this.Logger.WriteLine("Survey created successfully!");
			SurveyCreationSuccessEvent e = (this.ReceivedEvent as SurveyCreationSuccessEvent);

			// Initialize the survey with some "invalid" response
			await SurveyResponses.AddAsync(CurrentTransaction, e.SurveyId, -1);
		}

		/// <summary>
		/// When a survey completes, reliably store the results.
		/// </summary>
		/// <returns></returns>
		async Task RecordSurveyResults()
		{
			SurveyResultsEvent e = (this.ReceivedEvent as SurveyResultsEvent);
			int surveyId = e.SurveyId;
			int finalVotes = e.response;

			// Verify that the survey corresponds to a survey id received previously
			bool IsValidSurveyId = await SurveyResponses.ContainsKeyAsync(CurrentTransaction, e.SurveyId);
			this.Assert(IsValidSurveyId, "Survey ID is not valid");

			this.Logger.WriteLine("Subscriber ID: " + this.Id + " SurveyID: " + surveyId + " Votes: " + finalVotes);

			// Store the result
			await SurveyResponses.TryRemoveAsync(CurrentTransaction, surveyId);
			await SurveyResponses.AddAsync(CurrentTransaction, surveyId, finalVotes);

			// Unregister myself
			var tsCore = await TailSpinCoreMachine.Get();
			int subscriberId = await SubscriberId.Get();
			await this.ReliableSend(tsCore, new UnregisterSubscriberEvent(subscriberId));
		}

		#endregion

		#region methods

		protected override async Task OnActivate()
		{
			TailSpinCoreMachine = this.Host.GetOrAddRegister<IRsmId>("TSCoreMachine", null);
			SurveyResponses = await this.Host.GetOrAddAsync<IReliableDictionary<int, int>>("SurveyResponses");
			SubscriberId = this.Host.GetOrAddRegister<int>("SubscriberId", 0);
		}

		#endregion
	}
}