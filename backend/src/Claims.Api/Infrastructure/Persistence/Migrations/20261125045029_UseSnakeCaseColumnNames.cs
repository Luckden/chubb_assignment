using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Claims.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseSnakeCaseColumnNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_claim_status_changes_claims_ClaimId",
                table: "claim_status_changes");

            migrationBuilder.DropForeignKey(
                name: "FK_claim_status_changes_users_ChangedBy",
                table: "claim_status_changes");

            migrationBuilder.DropForeignKey(
                name: "FK_claims_users_AssigneeId",
                table: "claims");

            migrationBuilder.DropForeignKey(
                name: "FK_claims_users_ClaimantId",
                table: "claims");

            migrationBuilder.DropForeignKey(
                name: "FK_information_requests_claims_ClaimId",
                table: "information_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_information_requests_users_RequestedBy",
                table: "information_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_claims_ClaimId",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_users_UserId",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_OccurredAt_Id",
                table: "outbox_messages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_information_requests",
                table: "information_requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_claims",
                table: "claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_claim_status_changes",
                table: "claim_status_changes");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "users",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "users",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TeamId",
                table: "users",
                newName: "team_id");

            migrationBuilder.RenameColumn(
                name: "Topic",
                table: "outbox_messages",
                newName: "topic");

            migrationBuilder.RenameColumn(
                name: "Payload",
                table: "outbox_messages",
                newName: "payload");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "outbox_messages",
                newName: "key");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "outbox_messages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PublishedAt",
                table: "outbox_messages",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "outbox_messages",
                newName: "occurred_at");

            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "outbox_messages",
                newName: "event_type");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "notifications",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notifications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "ReadAt",
                table: "notifications",
                newName: "read_at");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "notifications",
                newName: "event_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "notifications",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ClaimId",
                table: "notifications",
                newName: "claim_id");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_UserId_CreatedAt",
                table: "notifications",
                newName: "ix_notifications_user_id_created_at");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_EventId",
                table: "notifications",
                newName: "ix_notifications_event_id");

            migrationBuilder.RenameIndex(
                name: "IX_notifications_ClaimId",
                table: "notifications",
                newName: "ix_notifications_claim_id");

            migrationBuilder.RenameColumn(
                name: "Response",
                table: "information_requests",
                newName: "response");

            migrationBuilder.RenameColumn(
                name: "Question",
                table: "information_requests",
                newName: "question");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "information_requests",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RespondedAt",
                table: "information_requests",
                newName: "responded_at");

            migrationBuilder.RenameColumn(
                name: "RequestedBy",
                table: "information_requests",
                newName: "requested_by");

            migrationBuilder.RenameColumn(
                name: "RequestedAt",
                table: "information_requests",
                newName: "requested_at");

            migrationBuilder.RenameColumn(
                name: "ClaimId",
                table: "information_requests",
                newName: "claim_id");

            migrationBuilder.RenameIndex(
                name: "IX_information_requests_RequestedBy",
                table: "information_requests",
                newName: "ix_information_requests_requested_by");

            migrationBuilder.RenameIndex(
                name: "IX_information_requests_ClaimId",
                table: "information_requests",
                newName: "ix_information_requests_claim_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "claims",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "claims",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "claims",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "claims",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SubmittedAt",
                table: "claims",
                newName: "submitted_at");

            migrationBuilder.RenameColumn(
                name: "IncidentDate",
                table: "claims",
                newName: "incident_date");

            migrationBuilder.RenameColumn(
                name: "DecisionReason",
                table: "claims",
                newName: "decision_reason");

            migrationBuilder.RenameColumn(
                name: "DecisionOutcome",
                table: "claims",
                newName: "decision_outcome");

            migrationBuilder.RenameColumn(
                name: "DecidedAt",
                table: "claims",
                newName: "decided_at");

            migrationBuilder.RenameColumn(
                name: "ClaimedAmount",
                table: "claims",
                newName: "claimed_amount");

            migrationBuilder.RenameColumn(
                name: "ClaimantId",
                table: "claims",
                newName: "claimant_id");

            migrationBuilder.RenameColumn(
                name: "ClaimNumber",
                table: "claims",
                newName: "claim_number");

            migrationBuilder.RenameColumn(
                name: "AssigneeId",
                table: "claims",
                newName: "assignee_id");

            migrationBuilder.RenameIndex(
                name: "IX_claims_Status_SubmittedAt",
                table: "claims",
                newName: "ix_claims_status_submitted_at");

            migrationBuilder.RenameIndex(
                name: "IX_claims_ClaimNumber",
                table: "claims",
                newName: "ix_claims_claim_number");

            migrationBuilder.RenameIndex(
                name: "IX_claims_ClaimantId",
                table: "claims",
                newName: "ix_claims_claimant_id");

            migrationBuilder.RenameIndex(
                name: "IX_claims_AssigneeId",
                table: "claims",
                newName: "ix_claims_assignee_id");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "claim_status_changes",
                newName: "note");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "claim_status_changes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ToStatus",
                table: "claim_status_changes",
                newName: "to_status");

            migrationBuilder.RenameColumn(
                name: "FromStatus",
                table: "claim_status_changes",
                newName: "from_status");

            migrationBuilder.RenameColumn(
                name: "ClaimId",
                table: "claim_status_changes",
                newName: "claim_id");

            migrationBuilder.RenameColumn(
                name: "ChangedBy",
                table: "claim_status_changes",
                newName: "changed_by");

            migrationBuilder.RenameColumn(
                name: "ChangedAt",
                table: "claim_status_changes",
                newName: "changed_at");

            migrationBuilder.RenameIndex(
                name: "IX_claim_status_changes_ClaimId",
                table: "claim_status_changes",
                newName: "ix_claim_status_changes_claim_id");

            migrationBuilder.RenameIndex(
                name: "IX_claim_status_changes_ChangedBy",
                table: "claim_status_changes",
                newName: "ix_claim_status_changes_changed_by");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_outbox_messages",
                table: "outbox_messages",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notifications",
                table: "notifications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_information_requests",
                table: "information_requests",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_claims",
                table: "claims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_claim_status_changes",
                table: "claim_status_changes",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_at_id",
                table: "outbox_messages",
                columns: new[] { "occurred_at", "id" },
                filter: "published_at IS NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_claim_status_changes_claims_claim_id",
                table: "claim_status_changes",
                column: "claim_id",
                principalTable: "claims",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_claim_status_changes_users_changed_by",
                table: "claim_status_changes",
                column: "changed_by",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_claims_users_assignee_id",
                table: "claims",
                column: "assignee_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_claims_users_claimant_id",
                table: "claims",
                column: "claimant_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_information_requests_claims_claim_id",
                table: "information_requests",
                column: "claim_id",
                principalTable: "claims",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_information_requests_users_requested_by",
                table: "information_requests",
                column: "requested_by",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_claims_claim_id",
                table: "notifications",
                column: "claim_id",
                principalTable: "claims",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_notifications_users_user_id",
                table: "notifications",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_claim_status_changes_claims_claim_id",
                table: "claim_status_changes");

            migrationBuilder.DropForeignKey(
                name: "fk_claim_status_changes_users_changed_by",
                table: "claim_status_changes");

            migrationBuilder.DropForeignKey(
                name: "fk_claims_users_assignee_id",
                table: "claims");

            migrationBuilder.DropForeignKey(
                name: "fk_claims_users_claimant_id",
                table: "claims");

            migrationBuilder.DropForeignKey(
                name: "fk_information_requests_claims_claim_id",
                table: "information_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_information_requests_users_requested_by",
                table: "information_requests");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_claims_claim_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "fk_notifications_users_user_id",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_outbox_messages",
                table: "outbox_messages");

            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_occurred_at_id",
                table: "outbox_messages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "pk_information_requests",
                table: "information_requests");

            migrationBuilder.DropPrimaryKey(
                name: "pk_claims",
                table: "claims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_claim_status_changes",
                table: "claim_status_changes");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "team_id",
                table: "users",
                newName: "TeamId");

            migrationBuilder.RenameColumn(
                name: "topic",
                table: "outbox_messages",
                newName: "Topic");

            migrationBuilder.RenameColumn(
                name: "payload",
                table: "outbox_messages",
                newName: "Payload");

            migrationBuilder.RenameColumn(
                name: "key",
                table: "outbox_messages",
                newName: "Key");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "outbox_messages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "published_at",
                table: "outbox_messages",
                newName: "PublishedAt");

            migrationBuilder.RenameColumn(
                name: "occurred_at",
                table: "outbox_messages",
                newName: "OccurredAt");

            migrationBuilder.RenameColumn(
                name: "event_type",
                table: "outbox_messages",
                newName: "EventType");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "notifications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "read_at",
                table: "notifications",
                newName: "ReadAt");

            migrationBuilder.RenameColumn(
                name: "event_id",
                table: "notifications",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "notifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "claim_id",
                table: "notifications",
                newName: "ClaimId");

            migrationBuilder.RenameIndex(
                name: "ix_notifications_user_id_created_at",
                table: "notifications",
                newName: "IX_notifications_UserId_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_notifications_event_id",
                table: "notifications",
                newName: "IX_notifications_EventId");

            migrationBuilder.RenameIndex(
                name: "ix_notifications_claim_id",
                table: "notifications",
                newName: "IX_notifications_ClaimId");

            migrationBuilder.RenameColumn(
                name: "response",
                table: "information_requests",
                newName: "Response");

            migrationBuilder.RenameColumn(
                name: "question",
                table: "information_requests",
                newName: "Question");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "information_requests",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "responded_at",
                table: "information_requests",
                newName: "RespondedAt");

            migrationBuilder.RenameColumn(
                name: "requested_by",
                table: "information_requests",
                newName: "RequestedBy");

            migrationBuilder.RenameColumn(
                name: "requested_at",
                table: "information_requests",
                newName: "RequestedAt");

            migrationBuilder.RenameColumn(
                name: "claim_id",
                table: "information_requests",
                newName: "ClaimId");

            migrationBuilder.RenameIndex(
                name: "ix_information_requests_requested_by",
                table: "information_requests",
                newName: "IX_information_requests_RequestedBy");

            migrationBuilder.RenameIndex(
                name: "ix_information_requests_claim_id",
                table: "information_requests",
                newName: "IX_information_requests_ClaimId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "claims",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "claims",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "claims",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "claims",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "submitted_at",
                table: "claims",
                newName: "SubmittedAt");

            migrationBuilder.RenameColumn(
                name: "incident_date",
                table: "claims",
                newName: "IncidentDate");

            migrationBuilder.RenameColumn(
                name: "decision_reason",
                table: "claims",
                newName: "DecisionReason");

            migrationBuilder.RenameColumn(
                name: "decision_outcome",
                table: "claims",
                newName: "DecisionOutcome");

            migrationBuilder.RenameColumn(
                name: "decided_at",
                table: "claims",
                newName: "DecidedAt");

            migrationBuilder.RenameColumn(
                name: "claimed_amount",
                table: "claims",
                newName: "ClaimedAmount");

            migrationBuilder.RenameColumn(
                name: "claimant_id",
                table: "claims",
                newName: "ClaimantId");

            migrationBuilder.RenameColumn(
                name: "claim_number",
                table: "claims",
                newName: "ClaimNumber");

            migrationBuilder.RenameColumn(
                name: "assignee_id",
                table: "claims",
                newName: "AssigneeId");

            migrationBuilder.RenameIndex(
                name: "ix_claims_status_submitted_at",
                table: "claims",
                newName: "IX_claims_Status_SubmittedAt");

            migrationBuilder.RenameIndex(
                name: "ix_claims_claimant_id",
                table: "claims",
                newName: "IX_claims_ClaimantId");

            migrationBuilder.RenameIndex(
                name: "ix_claims_claim_number",
                table: "claims",
                newName: "IX_claims_ClaimNumber");

            migrationBuilder.RenameIndex(
                name: "ix_claims_assignee_id",
                table: "claims",
                newName: "IX_claims_AssigneeId");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "claim_status_changes",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "claim_status_changes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "to_status",
                table: "claim_status_changes",
                newName: "ToStatus");

            migrationBuilder.RenameColumn(
                name: "from_status",
                table: "claim_status_changes",
                newName: "FromStatus");

            migrationBuilder.RenameColumn(
                name: "claim_id",
                table: "claim_status_changes",
                newName: "ClaimId");

            migrationBuilder.RenameColumn(
                name: "changed_by",
                table: "claim_status_changes",
                newName: "ChangedBy");

            migrationBuilder.RenameColumn(
                name: "changed_at",
                table: "claim_status_changes",
                newName: "ChangedAt");

            migrationBuilder.RenameIndex(
                name: "ix_claim_status_changes_claim_id",
                table: "claim_status_changes",
                newName: "IX_claim_status_changes_ClaimId");

            migrationBuilder.RenameIndex(
                name: "ix_claim_status_changes_changed_by",
                table: "claim_status_changes",
                newName: "IX_claim_status_changes_ChangedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_outbox_messages",
                table: "outbox_messages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notifications",
                table: "notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_information_requests",
                table: "information_requests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_claims",
                table: "claims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_claim_status_changes",
                table: "claim_status_changes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_OccurredAt_Id",
                table: "outbox_messages",
                columns: new[] { "OccurredAt", "Id" },
                filter: "\"PublishedAt\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_claim_status_changes_claims_ClaimId",
                table: "claim_status_changes",
                column: "ClaimId",
                principalTable: "claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_claim_status_changes_users_ChangedBy",
                table: "claim_status_changes",
                column: "ChangedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claims_users_AssigneeId",
                table: "claims",
                column: "AssigneeId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_claims_users_ClaimantId",
                table: "claims",
                column: "ClaimantId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_information_requests_claims_ClaimId",
                table: "information_requests",
                column: "ClaimId",
                principalTable: "claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_information_requests_users_RequestedBy",
                table: "information_requests",
                column: "RequestedBy",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_claims_ClaimId",
                table: "notifications",
                column: "ClaimId",
                principalTable: "claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_UserId",
                table: "notifications",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
